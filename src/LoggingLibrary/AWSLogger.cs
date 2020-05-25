using Amazon;
using Amazon.KinesisFirehose;
using Amazon.KinesisFirehose.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.Extensions.Logging;
using SSMLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace LoggingLibrary
{
    public class AWSLogger
    {
        private readonly LogInput m_BasicLogInput;
        private AmazonKinesisFirehoseClient m_Client;
        private readonly List<LogInput> m_InputsToSend;
        private DateTime m_ClientCreatedDateTime = DateTime.MinValue;
        private readonly string m_Feature;
        private readonly string m_Product;
        private ILogger m_logger = LogUtils.CreateLogger<AWSLogger>();
        private readonly bool m_shouldInitializeClient;

        private static int m_StreamIndex = 0;

        public AWSLogger()
        {
            m_InputsToSend = new List<LogInput>();
        }

        public AWSLogger(string i_OrgId, string i_OrgType, string i_InstanceName, string i_Environment,
            string i_Operation, string i_Product, string i_Feature)
        {
            m_Product = i_Product;
            m_Feature = i_Feature;
            m_BasicLogInput = new LogInput(i_OrgId, i_OrgType, i_InstanceName, i_Environment, i_Operation, i_Product,
                i_Feature);
            m_shouldInitializeClient = shouldInitializeFirehoseClient();
            updateAmazonKinesisFirehoseClient();
            m_InputsToSend = new List<LogInput>();
        }

        private bool shouldInitializeFirehoseClient()
        {
            bool result;

            try
            {
                string skipKibanaLogging = Environment.GetEnvironmentVariable("SkipKiabanaLogging");
                result = string.IsNullOrEmpty(skipKibanaLogging) || !bool.Parse(skipKibanaLogging);
            }
            catch (Exception ex)
            {
                result = true;
                m_logger.LogError(string.Format("Failed to decide on client initialization\n{0}", LogUtils.GetExceptionDetails(ex)));
            }

            return result;
        }

        private void updateAmazonKinesisFirehoseClient()
        {
            bool isOldClientNeedsRecreation = m_ClientCreatedDateTime == DateTime.MinValue ||
                DateTime.Now.Subtract(m_ClientCreatedDateTime) >= TimeSpan.FromHours(1);
            if (m_shouldInitializeClient && isOldClientNeedsRecreation)
            {
                try
                {
                    SSMParameter arnParameter = SSMClient.Instance.GetParameter("/FSL/Firehose/ARN");
                    string firehoseArn = arnParameter?.Value;

                    SSMParameter regionParameter = SSMClient.Instance.GetParameter(SSMLibrary.Constants.FSL_FIREHOSE_REGION_PATH);
                    RegionEndpoint firehoseRegionEndpoint = RegionEndpoint.GetBySystemName(regionParameter.Value);

                    if (string.IsNullOrEmpty(firehoseArn))
                    {
                        // Create client without assume role, same account as firehose.
                        m_Client = new AmazonKinesisFirehoseClient(firehoseRegionEndpoint);
                    }
                    else
                    {
                        // Create client with assume role
                        SSMParameter STSRegion = SSMClient.Instance.GetParameter("/FSL/Firehose/STSRegion");
                        AmazonSecurityTokenServiceClient stsClient = new AmazonSecurityTokenServiceClient(RegionEndpoint.GetBySystemName(STSRegion.Value));
                        AssumeRoleRequest stsRequest = new AssumeRoleRequest
                        {
                            RoleArn = firehoseArn,
                            RoleSessionName = string.Format("{0}_{1}_Monitoring", m_Product, m_Feature)
                        };
                        AssumeRoleResponse stsResult = stsClient.AssumeRoleAsync(stsRequest).Result;
                        m_Client = new AmazonKinesisFirehoseClient(stsResult.Credentials, firehoseRegionEndpoint);
                    }
                    m_ClientCreatedDateTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    m_logger.LogError(string.Format("Failed to initialize AWSLogger\n{0}", LogUtils.GetExceptionDetails(ex)));
                }
            }
        }

        public void AddDataToSend(string i_Context, string i_Message, int i_Count, LogLevel i_Level = LogLevel.Information)
        {
            AddDataToSend(i_Context, i_Message, i_Count, 0.0, null, i_Level);
        }

        public void AddDataToSend(string i_Context, string i_Message, double i_DurationInSeconds, LogLevel i_Level = LogLevel.Information)
        {
            AddDataToSend(i_Context, i_Message, 0, i_DurationInSeconds, null, i_Level);
        }

        public void AddDataToSend(string i_Context, string i_Message, string i_Details, LogLevel i_Level = LogLevel.Information)
        {
            AddDataToSend(i_Context, i_Message, 0, 0.0, i_Details, i_Level);
        }

        public void AddDataToSend(string i_Context, string i_Message, int i_Count, double i_DurationInSeconds, string i_Details, LogLevel i_Level = LogLevel.Information)
        {
            if (m_BasicLogInput != null)
            {
                LogInput dataToSend = (LogInput)m_BasicLogInput.Clone();
                dataToSend.Count = i_Count;
                dataToSend.Duration = i_DurationInSeconds;
                dataToSend.Details = i_Details;
                addDataToSendList(dataToSend, i_Context, i_Message, i_Level);
            }

        }

        private void addDataToSendList(LogInput i_Data, string i_Context, string i_Message, LogLevel i_Level)
        {

            try
            {
                i_Data.Context = i_Context;
                i_Data.Message = i_Message;
                i_Data.Level = i_Level;
                m_InputsToSend.Add(i_Data);
            }
            catch (Exception ex)
            {
                m_logger.LogError(string.Format("Failed to add monitoring statistics\n{0}", LogUtils.GetExceptionDetails(ex)));
                //m_logger.LogError(ex.Message);
            }
        }

        public void SendDataAsync()
        {
            if (m_InputsToSend != null && m_InputsToSend.Count > 0)
            {
                updateAmazonKinesisFirehoseClient();
                if (m_InputsToSend.Count == 1)
                {
                    sendSingleInputAsync();
                }
                else if (m_InputsToSend.Count > 1)
                {
                    sendBatchInputsAsync();
                }
                m_InputsToSend.Clear();
            }
        }

        private void sendSingleInputAsync()
        {
            try
            {
                string firehoseDelivaeryStreamName = getDeliveryStream();
                PutRecordRequest putRecord = new PutRecordRequest
                {
                    DeliveryStreamName = firehoseDelivaeryStreamName,
                    Record = createRecord(m_InputsToSend.First())
                };
                m_Client.PutRecordAsync(firehoseDelivaeryStreamName, putRecord.Record);
            }
            catch (Exception ex)
            {
                m_logger.LogError(string.Format("Failed to send monitoring statistics\n{0}", LogUtils.GetExceptionDetails(ex)));
                //m_logger.LogError("Error", "Failed to send monitoring statistics", ex.Message + "\n" + ex.StackTrace);
            }

        }

        private void sendBatchInputsAsync()
        {
            try
            {
                string firehoseDelivaeryStreamName = getDeliveryStream();

                PutRecordBatchRequest putRecordBatchRequest = new PutRecordBatchRequest
                {
                    DeliveryStreamName = firehoseDelivaeryStreamName,
                    Records = m_InputsToSend.Select(createRecord).ToList()
                };

                m_Client.PutRecordBatchAsync(putRecordBatchRequest);

            }
            catch (Exception ex)
            {
                m_logger.LogError(string.Format("Failed to send monitoring statistics\n{0}", LogUtils.GetExceptionDetails(ex)));
                //m_logger.LogError("Error", "Failed to send monitoring statistics", ex.Message + "\n" + ex.StackTrace);
            }
        }

        private static string getDeliveryStream()
        {
            string selectedDeliveryStream = string.Empty;
            SSMParameter streamsParameter = SSMClient.Instance.GetParameter(SSMLibrary.Constants.FSL_FIREHOSE_STREAM_PATH);
            if (streamsParameter != null && !string.IsNullOrEmpty(streamsParameter.Value))
            {
                string[] deliveryStreams = streamsParameter.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (deliveryStreams.Length > 0)
                {
                    m_StreamIndex %= deliveryStreams.Length;
                    selectedDeliveryStream = deliveryStreams[m_StreamIndex];
                    m_StreamIndex++;
                }
            }
            return selectedDeliveryStream;
        }

        private static Record createRecord(LogInput i_Input)
        {
            string data = i_Input.ToString();

            Record record = new Record()
            {
                Data = generateMemoryStreamFromString(data)
            };
            return record;
        }

        private static MemoryStream generateMemoryStreamFromString(string i_Data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(i_Data);
                    writer.Flush();
                    return stream;
                }
            }
        }

        public void AddExceptionToSend(Exception i_Ex)
        {
            AddErrorToSend(LogUtils.GetExceptionDetails(i_Ex));
        }

        public void AddErrorToSend(string i_ErrorMessage)
        {
            AddDataToSend("Error", "Error Message", i_ErrorMessage, LogLevel.Information);
        }
    }
}


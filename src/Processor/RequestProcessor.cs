using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Amazon.Lambda.Core;
using LoggingLibrary;
using Microsoft.Extensions.Logging;
using SalesforceLibrary.Requests;
using SalesforceLibrary.REST.FSL;
using SSMLibrary;

namespace Processor
{
    public static class RequestProcessor
    {
        private static FSLClient m_FSLClient;
        private static DataProcessor m_DataProcessor;

        private static SFDCScheduleRequest ParseRequestString(string i_RequestBody)
        {
            if (string.IsNullOrWhiteSpace(i_RequestBody))
                throw new ArgumentNullException("i_RequestBody");

            XmlDocument requestXML = new XmlDocument();
            requestXML.LoadXml(i_RequestBody);

            //TODO: for testing the regular BGO optimization request XML is used
            //TODO: in the future, a different XML should be used with only the relevant info to access SF
            Type requestType = typeof(AppointmentBookingRequest);
            XmlSerializer xmlSerializer = new XmlSerializer(requestType);

            object deserializedRequest;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(memoryStream))
                {
                    writer.Write(i_RequestBody);
                    writer.Flush();
                    memoryStream.Position = 0;
                    deserializedRequest = xmlSerializer.Deserialize(memoryStream);
                }
            }

            AppointmentBookingRequest optimizationRequest = deserializedRequest as AppointmentBookingRequest;
            return optimizationRequest;
        }

        public static void ProcessRequest(string i_RequestBody)
        {
            SFDCScheduleRequest request;

            request = ParseRequestString(i_RequestBody);

            //SSMClient.IntializeSSMParameters();
            //AWSLogger AWSLogger = createAwsLogger(request);
            //LogUtils.LoggerFactory.AddAWSLogger(AWSLogger, LogLevel.Information);

            connectToSF(request);

            m_DataProcessor = new DataProcessor(m_FSLClient, request);
            //ILogger logger = LogUtils.CreateLogger("Extract Data");
            Stopwatch watchExtractData = new Stopwatch();
            watchExtractData.Start();
            
            m_DataProcessor.ExtractData();
            
            watchExtractData.Stop();
            LambdaLogger.Log("\nExtraction of data by REST API took: " + watchExtractData.ElapsedMilliseconds + " ms\n");
            //logger.LogInformation(i_Context: "Data extraction", i_Message: "Querying AB data time using REST API", i_DurationInSeconds: watch.ElapsedMilliseconds);
            watchExtractData.Reset();
        }
        
        public static void GetDataByApexRestService(string i_RequestBody)
        {
            Stopwatch watchExtractDataApexRest = new Stopwatch();
            watchExtractDataApexRest.Start();
            
            SFDCScheduleRequest request;

            request = ParseRequestString(i_RequestBody);
            connectToSF(request);

            string responseData = m_FSLClient.RequestABData();
            watchExtractDataApexRest.Stop();

            LambdaLogger.Log("\nExtraction of data by APEX REST Service took: " + responseData + " ms\n");
            LambdaLogger.Log("Whole process by APEX REST Service including login to SF took: " + watchExtractDataApexRest.ElapsedMilliseconds +
                             " ms\n\n");
        
            watchExtractDataApexRest.Reset();
        }

        private static void connectToSF(SFDCScheduleRequest i_Request)
        {
            m_FSLClient = new FSLClient(i_Request.IsTest, i_Request.IsManaged, i_Request.OrganizationId);
            
            //SSMParameter clientIdParameter = SSMClient.Instance.GetParameter(SSMLibrary.Constants.FSL_SF_CLIENT_ID_PATH);
            //SSMParameter clientSecretParameter = SSMClient.Instance.GetParameter(SSMLibrary.Constants.FSL_SF_CLIENT_SECRET_PATH);

            string clientIdParameterStr =
                "3MVG9ZwZNrVajJ4juw533VQe450s71UT3VfQ.iBqsmmnxcogqAix1IusgXoHYMfR_xCRCWn9Gum5ICBFmeitF";
            string clientSecretParameterStr = "3591300309848784478";
            
            
            m_FSLClient.Login(clientIdParameterStr, clientSecretParameterStr, i_Request.RefreshToken, i_Request.CustomSFDCAuthURL);
        }

        private static AWSLogger createAwsLogger(SFDCScheduleRequest i_Request)
        {
            string environment = i_Request.IsTest ? "Sandbox" : "Production";

            string feature = "Unkown";
            
            return new AWSLogger(i_Request.OrganizationId, i_Request.OrganizationType,
                i_Request.InstanceName, environment, i_Request.AsyncIdentifier, "FSL", feature);
        }
    }
}


























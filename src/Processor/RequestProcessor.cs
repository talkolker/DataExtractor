using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Amazon.Lambda.Core;
using LoggingLibrary;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.Requests;
using SalesforceLibrary.REST.FSL;
using SSMLibrary;
using Formatting = Newtonsoft.Json.Formatting;

namespace Processor
{
    public static class RequestProcessor
    {
        private static FSLClient m_FSLClient;
        private static DataProcessor m_DataProcessor;
        private static bool m_IsApexRest = false;
        private static long m_SFLoginTime;

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

        public static string ProcessRequest(string i_RequestBody)
        {
            SFDCScheduleRequest request;

            request = ParseRequestString(i_RequestBody);

            connectToSF(request);

            string header = "\n~~~~~~~~ REST API ~~~~~~~~";

            m_DataProcessor = new DataProcessor(m_FSLClient, request);
            
            Stopwatch watchExtractData = new Stopwatch();
            watchExtractData.Start();
            
            RestAPIMeasurments measures = m_DataProcessor.ExtractData();
            string finalMeasures = JsonConvert.SerializeObject(measures.getMeasurments, Formatting.Indented);

            string log = header + "\nExtraction of data by REST API took: " + (watchExtractData.ElapsedMilliseconds - measures.MeasureToSubtract) +
                         " ms\nMeasurements per query:\n" + finalMeasures;
            watchExtractData.Stop();
            LambdaLogger.Log(log);
            //LambdaLogger.Log("Whole process by REST API including login to SF took: " + (watchExtractData.ElapsedMilliseconds + m_SFLoginTime) +" ms\n");
            watchExtractData.Reset();
            return log;
        }
        
        public static string GetDataByApexRestService(string i_RequestBody)
        {
            string header = "\n~~~~~~~~ APEX REST Service ~~~~~~~~";

            SFDCScheduleRequest request;

            request = ParseRequestString(i_RequestBody);
            connectToSF(request);
            
            Stopwatch watchExtractDataApexRest = new Stopwatch();
            watchExtractDataApexRest.Start();
            
            string responseData = m_FSLClient.RequestABData();
            DeserializedQueryResult deserializedResponse = JsonConvert.DeserializeObject<DeserializedQueryResult>(responseData);
            long elapsedTime = deserializedResponse.m_runtime;
            Dictionary<string, decimal> measurments = deserializedResponse.measures;
            watchExtractDataApexRest.Stop();

            string log = "\n\n" + header + "\nExtraction of data by APEX REST:\nExtraction in SFS MP: " + elapsedTime + " ms\n" +
                         "Request time (send request + get response) from AWS lambda to org: " + (watchExtractDataApexRest.ElapsedMilliseconds - elapsedTime) +
                             " ms\nMeasurements per query:\n" +
                         dictionaryToString(measurments) + "\n\n";
            LambdaLogger.Log(log);

            watchExtractDataApexRest.Reset();

            return log;
        }

        private static string dictionaryToString(Dictionary <string, decimal> dictionary) {  
            string dictionaryString = "{\n";  
            foreach(KeyValuePair <string, decimal> keyValues in dictionary) {  
                dictionaryString += "   " + keyValues.Key + " : " + keyValues.Value + "\n";  
            }  
            return dictionaryString.TrimEnd(',', ' ') + "}";  
        }

        private static void connectToSF(SFDCScheduleRequest i_Request)
        {
            m_FSLClient = new FSLClient(i_Request.IsTest, i_Request.IsManaged, i_Request.OrganizationId);
            
            //SSMParameter clientIdParameter = SSMClient.Instance.GetParameter(SSMLibrary.Constants.FSL_SF_CLIENT_ID_PATH);
            //SSMParameter clientSecretParameter = SSMClient.Instance.GetParameter(SSMLibrary.Constants.FSL_SF_CLIENT_SECRET_PATH);

            string clientIdParameterStr =
                "3MVG9ZwZNrVajJ4juw533VQe450s71UT3VfQ.iBqsmmnxcogqAix1IusgXoHYMfR_xCRCWn9Gum5ICBFmeitF";
            string clientSecretParameterStr = "3591300309848784478";
            
            Stopwatch watchSFConnection = new Stopwatch();
            watchSFConnection.Start();
            m_FSLClient.Login(clientIdParameterStr, clientSecretParameterStr, i_Request.RefreshToken, i_Request.CustomSFDCAuthURL);
            watchSFConnection.Stop();
            if (!m_IsApexRest)
            {
                //LambdaLogger.Log("\nConnection to SF took: " + watchSFConnection.ElapsedMilliseconds +
                //                 " ms");
                m_IsApexRest = true;
                m_SFLoginTime = watchSFConnection.ElapsedMilliseconds;
            }

            watchSFConnection.Reset();
        }

        private class DeserializedQueryResult
        {
            public Double? totalSize;
            public Boolean? done;
            public List<sObject> records;
            public long m_runtime;
            public Dictionary<string, decimal> measures;
        }
    }
}


























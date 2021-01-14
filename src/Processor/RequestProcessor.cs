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

            Stopwatch watch = new Stopwatch();
            watch.Start();
            RestAPIMeasurments measures = new RestAPIMeasurments();
            m_DataProcessor.ExtractData(measures);
            watch.Stop();
            long elapsedTime = watch.ElapsedMilliseconds;
            watch.Reset();
            
            for (int i = 0; i < 99; i++)
            {
                Stopwatch watchExtractData = new Stopwatch();
                watchExtractData.Start();
                RestAPIMeasurments currMeasures = new RestAPIMeasurments();
                m_DataProcessor.ExtractData(currMeasures);
                watchExtractData.Stop();
                mergeMeasures(measures, currMeasures);
                elapsedTime += watchExtractData.ElapsedMilliseconds;
                watchExtractData.Reset();
            }
            
            measures.updateMesurments();
            string finalMeasures = JsonConvert.SerializeObject(measures.getAverage(), Formatting.Indented);

            string log = header + "\nExtraction of data by REST API took (average of 100 calls): " + (elapsedTime/100) +
                         " ms\nMeasurements per query (average of 100 calls):\n" + finalMeasures + "\n\n";
            LambdaLogger.Log(log);
            //LambdaLogger.Log("Whole process by REST API including login to SF took: " + (watchExtractData.ElapsedMilliseconds + m_SFLoginTime) +" ms\n");
            return log;
        }

        private static void mergeMeasures(RestAPIMeasurments measures, RestAPIMeasurments currMeasures)
        {
            measures.getMeasurments[Measures.SA_PROCESSING] += currMeasures.getMeasurments[Measures.SA_PROCESSING];            
            measures.getMeasurments[Measures.DEPENDENCIES_PROCESSING] += currMeasures.getMeasurments[Measures.DEPENDENCIES_PROCESSING];
            measures.getMeasurments[Measures.MST_PROCESSING] += currMeasures.getMeasurments[Measures.MST_PROCESSING];
            measures.getMeasurments[Measures.STM_PROCESSING] += currMeasures.getMeasurments[Measures.STM_PROCESSING];
            measures.getMeasurments[Measures.PARENT_PROCESSING] += currMeasures.getMeasurments[Measures.PARENT_PROCESSING];
            measures.getMeasurments[Measures.VISITING_HOURS_PROCESSING] += currMeasures.getMeasurments[Measures.VISITING_HOURS_PROCESSING];
            measures.getMeasurments[Measures.RESOURCES_PROCESSING] += currMeasures.getMeasurments[Measures.RESOURCES_PROCESSING];
            measures.getMeasurments[Measures.UNLICENSED_USERS_PROCESSING] += currMeasures.getMeasurments[Measures.UNLICENSED_USERS_PROCESSING];
            measures.getMeasurments[Measures.CALENDARS_PROCESSING] += currMeasures.getMeasurments[Measures.CALENDARS_PROCESSING];

            measures.getMeasurments[Measures.SA_QUERY] += currMeasures.getMeasurments[Measures.SA_QUERY];
            measures.getMeasurments[Measures.DEPENDENCIES_QUERY] += currMeasures.getMeasurments[Measures.DEPENDENCIES_QUERY];
            measures.getMeasurments[Measures.STM_QUERY] += currMeasures.getMeasurments[Measures.STM_QUERY];
            measures.getMeasurments[Measures.PARENT_QUERY] += currMeasures.getMeasurments[Measures.PARENT_QUERY];
            measures.getMeasurments[Measures.RESOURCES_QUERY] += currMeasures.getMeasurments[Measures.RESOURCES_QUERY];
            measures.getMeasurments[Measures.UNLICENSED_USERS_QUERY] += currMeasures.getMeasurments[Measures.UNLICENSED_USERS_QUERY];
            measures.getMeasurments[Measures.CALENDARS_QUERY] += currMeasures.getMeasurments[Measures.CALENDARS_QUERY];

            measures.getMeasurments[Measures.OBJECTIVES_RULES_PARALLEL] += currMeasures.getMeasurments[Measures.OBJECTIVES_RULES_PARALLEL];
            measures.getMeasurments[Measures.ADITTIONAL_DATA_PARALLEL] += currMeasures.getMeasurments[Measures.ADITTIONAL_DATA_PARALLEL];
        }

        public static string GetDataByApexRestService(string i_RequestBody)
        {
            string header = "\n~~~~~~~~ APEX REST Service ~~~~~~~~";

            SFDCScheduleRequest request;

            request = ParseRequestString(i_RequestBody);
            connectToSF(request);
            
            long elapsedTime = 0;
            Dictionary<string, decimal> ongoingMeasurments = new Dictionary<string, decimal>();
            long totalExtraction = 0;

            for (int i = 0; i < 100; i++)
            {
                Stopwatch watchExtractDataApexRest = new Stopwatch();
                watchExtractDataApexRest.Start();
                
                string responseData = m_FSLClient.RequestABData();
                DeserializedQueryResult deserializedResponse =
                    JsonConvert.DeserializeObject<DeserializedQueryResult>(responseData);
                elapsedTime += deserializedResponse.m_runtime;
                Dictionary<string, decimal> measurments = deserializedResponse.measures;
                
                watchExtractDataApexRest.Stop();
                totalExtraction += watchExtractDataApexRest.ElapsedMilliseconds;
                watchExtractDataApexRest.Reset();
                mergeMeasures(ongoingMeasurments, measurments);
            }

            Dictionary<string, decimal> finalMeasurments = getAverageResults(ongoingMeasurments);
            string log = "\n\n" + header + "\nExtraction of data by APEX REST (average of 100 calls):\nExtraction in SFS MP: " + (elapsedTime/100) + " ms\n" +
                         "Total extraction of data by REST API took: " + (totalExtraction/100) +
                         " ms\nMeasurements per query:\n" +
                         dictionaryToString(finalMeasurments) + "\n\n";
            LambdaLogger.Log(log);

            return log;
        }

        private static Dictionary<string, decimal> getAverageResults(Dictionary<string, decimal> measurments)
        {
            Dictionary<string, decimal> finalMeasurments = new Dictionary<string, decimal>();
            
            foreach (string key in measurments.Keys)
            {
                finalMeasurments[key] = measurments[key] / 100;
            }

            return finalMeasurments;
        }

        private static void mergeMeasures(Dictionary<string, decimal> finalMeasurments, Dictionary<string, decimal> currMeasures)
        {
            foreach (string key in currMeasures.Keys)
            {
                if (!finalMeasurments.ContainsKey(key))
                {
                    finalMeasurments[key] = 0;
                }

                finalMeasurments[key] += currMeasures[key];
            }
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

        public static string SendEmptyRequest(string i_RequestBody)
        {
            string header = "\n~~~~~~~~ REST API Empty Request ~~~~~~~~\n";

            SFDCScheduleRequest request;

            request = ParseRequestString(i_RequestBody);
            connectToSF(request);
            long elapsedTime = 0;
            for (int i = 0; i < 100; i++)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                bool success = m_FSLClient.SendEmptyRequest();
                if (!success)
                {
                    throw new Exception("failed to send empty request");
                }
                watch.Stop();

                elapsedTime += watch.ElapsedMilliseconds;
                watch.Reset();
            }

            string result = header + "Average of 100 calls: " + (elapsedTime / 100) + " ms\n\n";
            LambdaLogger.Log(result);
            return result;
        }
    }
}


























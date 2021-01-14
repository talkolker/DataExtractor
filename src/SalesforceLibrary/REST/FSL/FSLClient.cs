using LoggingLibrary;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Diagnostics;
using System.Net.Cache;
using Microsoft.AspNetCore.Http.Features;
using Processor;


namespace SalesforceLibrary.REST.FSL
{
    public class FSLClient : SalesforceClient
    {
        private const string OaaS_Rest_Service_Endpoint = "OAASRestService/";
        private const string OaaS_Rest_Insights_Endpoint = "OAASRestInsights/";
        private const string ABData_Rest_Service_Endpoint = "ABService/";
        private string m_DataUrl;

        private readonly bool m_IsManaged;
        //private readonly string m_DataIdentifier;
        private RestClient m_Client;
        private ILogger m_logger;

        public FSLClient(bool i_IsSandbox, bool i_IsManaged, string i_ordId) :
            base(i_IsSandbox, i_ordId)
        {
            m_IsManaged = i_IsManaged;
            //m_DataIdentifier = i_DataIdentifier;
            m_logger = LogUtils.CreateLogger<FSLClient>();
        }

        protected override void afterLoginSuccess()
        {
            string servicesUrl = m_SFToken.URL + SF_Apex_Rest_Services_Endpoint; 
            //m_DataUrl = m_SFToken.URL + SF_Apex_Rest_Data_Endpoint;
            if (m_IsManaged)
                servicesUrl += "FSL/";
            m_Client = new RestClient(m_SFToken.URL);
            m_Client.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);
            //m_Client.AddDefaultHeader("Authorization", "OAuth " + m_SFToken.AccessToken);
        }
        
        public string ExecuteQuery(string i_Query, string i_MeasureType, Measurments i_Mesurements,
            bool i_NextRecords = false, bool async = false)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            if (i_NextRecords)
            {
                RestRequest restApiRequestNextRecords = new RestRequest(i_Query, Method.GET);
                restApiRequestNextRecords.AddHeader("Authorization", "Bearer " + m_SFToken.AccessToken);
                restApiRequestNextRecords.AddHeader("Accept-Encoding", "gzip");
                IRestResponse<string> restApiResponseNextRecords = m_Client.Execute<string>(restApiRequestNextRecords);

                return restApiResponseNextRecords.Data;
            }
            
            string requestQuery = SF_Apex_Rest_Data_Endpoint + i_Query;
            RestRequest restApiRequest = new RestRequest(requestQuery, Method.GET);
            restApiRequest.AddHeader("Authorization", "Bearer " + m_SFToken.AccessToken);
            restApiRequest.AddHeader("Accept-Encoding", "gzip");

            try
            {
                IRestResponse<string> restApiResponse = m_Client.Execute<string>(restApiRequest);

                if (restApiResponse.IsSuccessful)
                    return restApiResponse.Data;
                else
                {
                    //TODO: throw relevant exception
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Network failure", ex);
            }
            finally
            {
                watch.Stop();
                
                if(!async)
                    i_Mesurements.addMeasure(i_MeasureType, watch.ElapsedMilliseconds);
                
                watch.Reset();
            }
        }
        
        //For apex rest service
        public string RequestABData(RestRequest restRequest)
        {
            try
            {
                IRestResponse<string> restApiResponse = m_Client.Execute<string>(restRequest);

                if (restApiResponse.IsSuccessful)
                    return restApiResponse.Data;
                else
                {
                    //TODO: throw relevant exception
                    throw new Exception();
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Network failure", ex);
            }
        }
        
        public RestRequest getApexRequest()
        {
            RestRequest restApiRequest = new RestRequest(ABData_Rest_Service_Endpoint, Method.GET);
            restApiRequest.AddHeader("Accept-Encoding", "gzip");

            return restApiRequest;
        }

        public bool SendEmptyRequest(RestRequest restRequest)
        {
            try
            {
                IRestResponse<string> restApiResponse = m_Client.Execute<string>(restRequest);
                if (restApiResponse.IsSuccessful)
                    return true;
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Network failure", ex);
            }
        }

        public void setClientURI()
        {
            string servicesUrl = m_SFToken.URL + SF_Apex_Rest_Services_Endpoint;
            if (m_IsManaged)
                servicesUrl += "FSL/";
            m_Client.BaseUrl = new Uri(servicesUrl);
            m_Client.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);
        }
        
        public void setClientURIforApexRest()
        {
            string servicesUrl = m_SFToken.URL + SF_Apex_Rest_Services_Endpoint;
            if (m_IsManaged)
                servicesUrl += "FSL/";
            m_Client.BaseUrl = new Uri(servicesUrl);
            m_Client.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);
            m_Client.AddDefaultHeader("Authorization", "OAuth " + m_SFToken.AccessToken);
        }

        public RestRequest getEmptyRequest()
        {
            //RestRequest restApiRequest = new RestRequest(ABData_Rest_Service_Endpoint, Method.POST);
            RestRequest restApiRequest = new RestRequest("EmptyRest/", Method.GET);
            restApiRequest.AddHeader("Authorization", "Bearer " + m_SFToken.AccessToken);
            restApiRequest.AddHeader("Accept-Encoding", "gzip");

            return restApiRequest;
        }
    }
}

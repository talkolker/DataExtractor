using LoggingLibrary;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SalesforceLibrary.DataModel;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.DataModel.Utils;
using SalesforceLibrary.Exceptions;
using SalesforceLibrary.REST.FSL.OaaS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TransactionLibrary;

namespace SalesforceLibrary.REST.FSL
{
    public class FSLClient : SalesforceClient
    {
        private const string OaaS_Rest_Service_Endpoint = "OAASRestService/";
        private const string OaaS_Rest_Insights_Endpoint = "OAASRestInsights/";
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
            //m_Client.AddDefaultHeader("Authorization", "OAuth " + m_SFToken.AccessToken);
        }
        
        public string ExecuteQuery(string i_Query, bool i_NextRecords = false)
        {
            if (i_NextRecords)
            {
                RestRequest restApiRequestNextRecords = new RestRequest(i_Query, Method.GET);
                restApiRequestNextRecords.AddHeader("Authorization", "Bearer " + m_SFToken.AccessToken);
                IRestResponse<string> restApiResponseNextRecords = m_Client.Execute<string>(restApiRequestNextRecords);

                return restApiResponseNextRecords.Data;
            }
            
            string requestQuery = SF_Apex_Rest_Data_Endpoint + i_Query;
            RestRequest restApiRequest = new RestRequest(requestQuery, Method.GET);
            restApiRequest.AddHeader("Authorization", "Bearer " + m_SFToken.AccessToken);

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
            catch(Exception ex)
            {
                throw new Exception("Network failure", ex);
            }
        }

    }
}

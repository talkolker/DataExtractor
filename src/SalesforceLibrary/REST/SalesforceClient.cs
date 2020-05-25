using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using SalesforceLibrary.Exceptions;
using LoggingLibrary;
using Microsoft.Extensions.Logging;
using TransactionLibrary;
using SSMLibrary;

namespace SalesforceLibrary.REST
{
    public class SalesforceClient
    {
        protected const string SF_Token_Service_Endpoint = "/services/oauth2/token";
        protected const string SF_Token_Introspection_Endpoint = "/services/oauth2/introspect";
        protected const string SF_Apex_Rest_Services_Endpoint = "/services/apexrest/";
        protected const string SF_Apex_Rest_Data_Endpoint = "/services/data/v48.0/query/?q=";
        protected static readonly DateTime epoch = new DateTime(1970, 1, 1);
        protected bool m_IsSandbox;
        protected SFTokenServiceResponse m_SFToken;
        protected SFTokenIntrospectionResponse m_SFTokenInfo;
        protected readonly ILogger m_Logger;
        protected static readonly int NUMBER_OF_RETRIES = int.Parse(SSMClient.Instance.GetParameter(SSMLibrary.Constants.POD_RETRY_MAX_NUMBER_OF_TRIES).Value);
        protected static readonly int DELAY_BETWEEN_TRIES_MILLISECONDS = int.Parse(SSMClient.Instance.GetParameter(SSMLibrary.Constants.POD_RETRY_INTERVAL).Value);

        public bool ReadOnlyMode { get; set; }
        //private bool m_IsSQSInfrastructure;
        protected string m_OrgId;
        
        public SalesforceClient(bool i_IsSandbox, string i_orgId)
        {
            m_Logger = LogUtils.CreateLogger<SalesforceClient>();
            m_IsSandbox = i_IsSandbox;
            ReadOnlyMode = false;
            m_OrgId = i_orgId;
        }

        public void Login(string i_ClientID, string i_ClientSecret, string i_RefreshToken, string i_CustomURL)
        {
            //m_IsSQSInfrastructure = i_IsSQSInfrastructure;
            string sfTokenServiceUrl = string.IsNullOrEmpty(i_CustomURL) ? string.Format("https://{0}.salesforce.com", m_IsSandbox ? "test" : "login") : i_CustomURL;
            
            using (HttpClient client = new HttpClient())
            {
                m_Logger.LogTrace("SalesforceClient","Getting tokens from {0}", sfTokenServiceUrl);
                client.BaseAddress = new Uri(sfTokenServiceUrl);
                FormUrlEncodedContent requestContent = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string,string>("grant_type", "refresh_token"),
                    new KeyValuePair<string,string>("client_id", i_ClientID),
                    new KeyValuePair<string,string>("client_secret", i_ClientSecret),
                    new KeyValuePair<string,string>("refresh_token", i_RefreshToken),
                    new KeyValuePair<string,string>("format", "json"),
                });

                getAccessToSF(client, requestContent, i_ClientID, i_ClientSecret, i_Login: true);
            };
        }

        private void getAccessToSF(HttpClient i_Client, FormUrlEncodedContent i_RequestContent, string i_ClientID, string i_ClientSecret, bool i_Login)
        {
            int currentRetry = 0;
            for (;;)
            {
                //if (m_IsSQSInfrastructure)
                //{
                //    m_Logger.LogTrace("before get token from redis");
                //    bool tokenExists = getTokenFromRedis();
                //    m_Logger.LogTrace("after get token from redis. token exists: " + tokenExists);
                //    if (tokenExists)
                //    {
                //        afterLoginSuccess();
                //        break;
                //    }
                //}

                try
                {
                    // try to login to SFDC
                    string endpoint = i_Login ? SF_Token_Service_Endpoint : SF_Token_Introspection_Endpoint;
                    m_Logger.LogTrace("introspection endpoint: " + endpoint);
                    HttpResponseMessage response = i_Client.PostAsync(endpoint, i_RequestContent).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        if (i_Login)
                            loginToSF(i_Client, response, i_ClientID, i_ClientSecret);
                        //else if(m_IsSQSInfrastructure)
                        //    inspectSFToken(response);
                        
                        // if successful break the loop
                        break;
                    }
                    else
                    {
                        m_Logger.LogTrace("Failed logging to Salesforce {0} servers", m_IsSandbox ? "Sandbox" : "Production");
                        currentRetry++;
                        if (currentRetry < NUMBER_OF_RETRIES)
                        {
                            Thread.Sleep(DELAY_BETWEEN_TRIES_MILLISECONDS);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
                catch (Exception ex)
                {
                    m_Logger.LogWarning("Failed logging to Salesforce {0} servers\n" + LogUtils.GetExceptionDetails(ex), m_IsSandbox ? "Sandbox" : "Production");
                    currentRetry++;
                    if (currentRetry < NUMBER_OF_RETRIES)
                    {
                        Thread.Sleep(DELAY_BETWEEN_TRIES_MILLISECONDS);
                    }
                    else
                    {
                        throw new SFGeneralException("Cannot login to salesforce");
                    }
                }
            }
        }

        private void inspectSFToken(HttpResponseMessage i_ResponseIntrospective)
        {
            string responseIntrospectiveContent = i_ResponseIntrospective.Content.ReadAsStringAsync().Result;
            m_SFTokenInfo =
                JsonConvert.DeserializeObject<SFTokenIntrospectionResponse>(responseIntrospectiveContent);
            m_SFTokenInfo.AccessToken = m_SFToken.AccessToken;
            m_SFTokenInfo.URL = m_SFToken.URL;

            TimeSpan access_token_ttl = (m_SFTokenInfo.ExpireAt - DateTime.UtcNow) * 0.9;
            var storeTokenResult = ECClient.Instance.PutSFTokenAsync( m_OrgId,  JsonConvert.SerializeObject(m_SFTokenInfo), access_token_ttl).Result;
    
            if (!storeTokenResult.Success)
            {
                m_Logger.LogInformation("Redis Operation", "Cannot put data in Redis: " + ECClient.Instance.HostName);   
            }
        }

        private void loginToSF(HttpClient i_Client, HttpResponseMessage i_Response, string i_ClientID, string i_ClientSecret)
        {
            string responseContent = i_Response.Content.ReadAsStringAsync().Result;
            m_Logger.LogTrace("Login service response: {0}", responseContent);
            m_SFToken = JsonConvert.DeserializeObject<SFTokenServiceResponse>(responseContent);

            //if (m_IsSQSInfrastructure)
            //{
            //    insertTokenToRedis(i_Client, i_ClientSecret, i_ClientID);
            //}

            afterLoginSuccess();
        }

        protected virtual void afterLoginSuccess() { }

        [JsonObject]
        protected class SFTokenServiceResponse
        {

            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("instance_url")]
            public string URL { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("issued_at")]
            private long issued_at { get; set; }

            public DateTime IssuedAt { get { return epoch.AddSeconds(issued_at); } }

            [JsonProperty("signature")]
            public string Signature { get; set; }
        }
        
        [JsonObject]
        protected class SFTokenIntrospectionResponse
        {

            [JsonProperty("token_type")]
            public string tokenType { get; set; }

            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("instance_url")]
            public string URL { get; set; }
            
            [JsonProperty("iat")]
            private long issued_at { get; set; }
            
            [JsonProperty("exp")]
            private long expire_at { get; set; }
            
            public DateTime ExpireAt { get { return epoch.AddSeconds(expire_at); } }

            public DateTime IssuedAt { get { return epoch.AddSeconds(issued_at); } }
        }
    }
}

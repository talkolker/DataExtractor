using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System.Net;
using Microsoft.Extensions.Logging;
using Amazon;

namespace SSMLibrary
{
    public sealed class SSMClient
    {
        private static readonly SSMClient m_Instance;

        static SSMClient()
        {
            m_Instance = new SSMClient();
        }

        public static SSMClient Instance
        {
            get
            {
                return m_Instance;
            }
        }

        private readonly ILogger m_Logger;
        private readonly AmazonSimpleSystemsManagementClient m_Client;

        private Dictionary<string, SSMParameter> m_Parameters;
        private HashSet<string> m_InvalidParameters;

        public TimeSpan MaxAge { get; set; }

        private SSMClient()
        {
            MaxAge = TimeSpan.FromDays(1);
            m_Parameters = new Dictionary<string, SSMParameter>(StringComparer.OrdinalIgnoreCase);
            m_InvalidParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            m_Logger = new LoggerFactory().CreateLogger<SSMClient>();
            try
            {
                string aws_ps_region = Environment.GetEnvironmentVariable("AWS_Region");
                if (string.IsNullOrWhiteSpace(aws_ps_region))
                {
                    m_Client = new AmazonSimpleSystemsManagementClient();
                }
                else
                {
                    RegionEndpoint parameterStoreRegion = RegionEndpoint.GetBySystemName(aws_ps_region);
                    m_Client = new AmazonSimpleSystemsManagementClient(parameterStoreRegion);
                }
            }
            catch (Exception ex)
            {
                m_Logger.LogInformation("Exception info", ex.StackTrace);
                m_Logger.LogDebug("SSMClient", "Cannot create SSM Client: {0}", ex.Message);
                m_Client = null;
            }
        }

        public void Initialize(List<string> i_Parameters)
        {
            getParametersFromSSM(i_Parameters);
        }

        private void getParametersFromSSM(IEnumerable<string> i_Parameters)
        {
            if (m_Client != null)
            {
                if (i_Parameters != null && i_Parameters.Any())
                {
                    for (int parametersIndex = 0; parametersIndex < i_Parameters.Count(); parametersIndex += 10)
                    {
                        IEnumerable<string> partialParametersList = i_Parameters.Skip(parametersIndex).Take(10);
                        if (partialParametersList != null && partialParametersList.Any())
                        {
                            GetParametersRequest parametersRequest = buildRequest(partialParametersList);
                            GetParametersResponse parametersResponse = m_Client.GetParametersAsync(parametersRequest).Result;
                            processGetParametersResponse(parametersResponse);
                        }
                    }
                }
            }
        }

        private GetParametersRequest buildRequest(IEnumerable<string> i_Parameters)
        {
            GetParametersRequest request = new GetParametersRequest
            {
                Names = i_Parameters.ToList(),
                WithDecryption = true
            };
            return request;
        }

        private void processGetParametersResponse(GetParametersResponse i_Response)
        {
            if (i_Response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new ArgumentException("Could not get parameters from SSM", i_Response.HttpStatusCode.ToString());
            }

            if (i_Response.InvalidParameters != null && i_Response.InvalidParameters.Any())
            {
                m_InvalidParameters.UnionWith(i_Response.InvalidParameters);
            }

            if (i_Response.Parameters != null && i_Response.Parameters.Any())
            {
                i_Response.Parameters.ForEach(awsParameter => m_Parameters[awsParameter.Name] = new SSMParameter(awsParameter));
            }
        }

        public SSMParameter GetParameter(string i_Name)
        {
            return getParameter(i_Name, i_TryFetchinFromSSM: true);
        }

        private SSMParameter getParameter(string i_Name, bool i_TryFetchinFromSSM)
        {
            if (m_InvalidParameters.Contains(i_Name))
            {
                throw new ArgumentException("Parameter name is invalid", i_Name);
            }

            SSMParameter resultParameter = null;
            if (m_Parameters.TryGetValue(i_Name, out resultParameter))
            {
                if (resultParameter.LastFetched + MaxAge < DateTime.Now)
                {
                    if (i_TryFetchinFromSSM)
                    {
                        getParametersFromSSM(m_Parameters.Keys);
                        resultParameter = getParameter(i_Name, i_TryFetchinFromSSM: false);
                    }
                    else
                    {
                        resultParameter = getParameterFromEnvironment(i_Name);
                    }
                }
            }
            else
            {
                resultParameter = getParameterFromEnvironment(i_Name);
            }

            return resultParameter;
        }

        private SSMParameter getParameterFromEnvironment(string i_Name)
        {
            SSMParameter resultParameter = null;
            string environmentVariableName = i_Name.Trim('/').Replace('/', '_');

            if (!string.IsNullOrEmpty(environmentVariableName))
            {
                string environmentVariableValue = Environment.GetEnvironmentVariable(environmentVariableName);
                if (!string.IsNullOrEmpty(environmentVariableValue))
                {
                    Parameter awsParameter = new Parameter
                    {
                        Name = i_Name,
                        Value = environmentVariableValue,
                        Type = ParameterType.String
                    };
                    resultParameter = new SSMParameter(awsParameter);
                }
            }

            return resultParameter;

        }
        
        public static void IntializeSSMParameters()
        {
            List<string> parametersToFetchFromSSM = new List<string>
            {
                Constants.FSL_SF_CLIENT_ID_PATH,
                Constants.FSL_SF_CLIENT_SECRET_PATH,
                Constants.FSL_FIREHOSE_REGION_PATH,
                Constants.FSL_FIREHOSE_STREAM_PATH,
                Constants.POD_SQS_REGION_PATH,
                Constants.POD_SQS_IN_SHORT_PATH,
                Constants.POD_SQS_IN_LONG_PATH,
                Constants.POD_ELASTICACHE_REDIS_URL_PATH,
                Constants.POD_ELASTICACHE_REDIS_PORT_PATH,
                Constants.POD_ELASTICACHE_MEMCACHED_URL_PATH,
                Constants.POD_ELASTICACHE_MEMCACHED_PORT_PATH,
                Constants.POD_RETRY_INTERVAL,
                Constants.POD_RETRY_MAX_NUMBER_OF_TRIES,
                Constants.POD_GIS_AUTH,
                Constants.POD_SQS_INSIGHTS_PATH
            };
            Instance.Initialize(parametersToFetchFromSSM);
        }
    }
}
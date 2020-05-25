using LoggingLibrary;
using Microsoft.Extensions.Logging;
using SSMLibrary;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using TransactionLibrary.Abstraction;
using TransactionLibrary.Enums;

namespace TransactionLibrary.Engines.Clients
{
    internal sealed class RedisClient : IAsyncECClient
    {

        private const string EXCEPTION_MESSAGE = "{0} {1} {2}";
        private const string FUNCTION_CONTEXT_FORMAT = "Redis Client, Function: {0} with key: {1}";

        private readonly ILogger m_Logger;
        private readonly ConnectionMultiplexer m_redis;
        private readonly IDatabase m_db;

        public string HostName { get; private set; }
        public int Port { get; private set; }

        public eECEngineType EngineType => eECEngineType.Redis;

        internal RedisClient()
        {
            LoggerFactory logFactory = new LoggerFactory();
            logFactory.AddConsole(true);
            m_Logger = logFactory.CreateLogger<RedisClient>();

            initializeParameters();

            using (m_Logger.BeginScope("Creating client '{0}:{1}'", HostName, Port))
            {
                try
                {
                    ConfigurationOptions redisConfig = new ConfigurationOptions()
                    {
                        AbortOnConnectFail = false,
                        ClientName = Environment.MachineName,
                        EndPoints =
                        {
                            {HostName,Port }
                        }
                    };
                    m_redis = ConnectionMultiplexer.Connect(redisConfig);
                    m_db = m_redis.GetDatabase();
                    m_Logger.LogInformation("Redis client created");
                }
                catch (Exception ex)
                {
                    m_Logger.LogError("Redis Init", "Redis client creation failed.", LogUtils.GetExceptionDetails(ex));
                }
            }
        }

        private void initializeParameters()
        {
            SSMParameter hostnameParameter = SSMClient.Instance.GetParameter(SSMLibrary.Constants.POD_ELASTICACHE_REDIS_URL_PATH);
            HostName = hostnameParameter.Value;

            SSMParameter portParameter = SSMClient.Instance.GetParameter(SSMLibrary.Constants.POD_ELASTICACHE_REDIS_PORT_PATH);
            Port = int.Parse(portParameter.Value);
        }

        public async Task<string> GetStringAsync(string i_Key)
        {
            string getActionResult;

            try
            {
                m_Logger.LogInformation(string.Format(FUNCTION_CONTEXT_FORMAT, "GetStringAsync", i_Key));
                var cacheObject = await m_db.StringGetAsync(i_Key);
                getActionResult = cacheObject.ToString();
            }
            catch (Exception ex)
            {
                m_Logger.LogInformation(string.Format(EXCEPTION_MESSAGE, EngineType, "GetStringAsync", LogUtils.GetExceptionDetails(ex)));
                getActionResult = string.Empty;
            }

            return getActionResult;
        }

        public async Task<bool> PutStringAsync(string i_Key, string i_Value, TimeSpan i_TTL)
        {
            bool putActionResult;

            try
            {
                m_Logger.LogInformation(string.Format(FUNCTION_CONTEXT_FORMAT, "PutStringAsync", i_Key));
                putActionResult = await m_db.StringSetAsync(i_Key, i_Value, i_TTL);
                //string cacheResult = await GetStringAsync(i_Key);
                //putActionResult = !string.IsNullOrEmpty(cacheResult);
            }
            catch (Exception ex)
            {
                m_Logger.LogInformation(string.Format(EXCEPTION_MESSAGE, EngineType, "PutStringAsync", LogUtils.GetExceptionDetails(ex)));
                putActionResult = false;
            }

            return putActionResult;
        }

        public async Task<bool> DeleteKeyAsync(string i_Key)
        {
            m_Logger.LogInformation(string.Format(FUNCTION_CONTEXT_FORMAT, "DeleteKey", i_Key));
            return await m_db.KeyDeleteAsync(i_Key);
        }
    }
}
using Enyim.Caching;
using Enyim.Caching.Configuration;
using LoggingLibrary;
using Microsoft.Extensions.Logging;
using SSMLibrary;
using System;
using System.Threading.Tasks;
using TransactionLibrary.Abstraction;
using TransactionLibrary.Enums;

namespace TransactionLibrary.Engines.Clients
{
    internal sealed class MemCachedClient : IAsyncECClient
    {

        private const string EXCEPTION_MESSAGE = "{0} {1} {2}";
        private const string FUNCTION_CONTEXT_FORMAT = "MemCached Client, Function: {0} with key: {1}";

        private readonly ILogger m_Logger;
        private readonly MemcachedClient m_Client;

        internal MemCachedClient()
        {
            LoggerFactory logFactory = new LoggerFactory();
            logFactory.AddConsole(LogLevel.Information, true);
            m_Logger = logFactory.CreateLogger<MemCachedClient>();

            initializeParameters();

            using (m_Logger.BeginScope("Creating client '{0}:{1}'", HostName, Port))
            {
                try
                {
                    MemcachedClientConfiguration baseConfig = null;
                    MemcachedClientOptions configOptions = new MemcachedClientOptions();
                    configOptions.AddServer(HostName, Port);
                    configOptions.Protocol = Enyim.Caching.Memcached.MemcachedProtocol.Text;

                    baseConfig = new MemcachedClientConfiguration(new LoggerFactory(), configOptions);

                    m_Client = new MemcachedClient(new LoggerFactory(), baseConfig);

                    m_Logger.LogDebug("Memory cached client created.");
                }
                catch (Exception ex)
                {
                    m_Logger.LogError(ex, "Memory cache client creation failed.");
                }
            }
        }

        public string HostName { get; private set; }

        public int Port { get; private set; }

        public eECEngineType EngineType => eECEngineType.MemCached;

        public async Task<bool> DeleteKeyAsync(string i_Key)
        {
            try
            {
                m_Logger.LogInformation(string.Format(FUNCTION_CONTEXT_FORMAT, "DeleteKey", i_Key));
                return await m_Client.RemoveAsync(i_Key);
            }
            catch (Exception ex)
            {
                m_Logger.LogError(string.Format(EXCEPTION_MESSAGE, EngineType, "DeleteKey",LogUtils.GetExceptionDetails(ex)));
                throw;
            }
        }

        public async Task<string> GetStringAsync(string i_Key)
        {
            string getActionResult;

            try
            {
                m_Logger.LogInformation(string.Format(FUNCTION_CONTEXT_FORMAT, "GetStringAsync", i_Key));
                var cacheObject = await m_Client.GetAsync<string>(i_Key);
                getActionResult = cacheObject.Value;
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
                int ttlInSeconds = Convert.ToInt32(i_TTL.TotalSeconds);
                await m_Client.AddAsync(i_Key, i_Value, ttlInSeconds);
                var cacheResult = await m_Client.GetAsync<string>(i_Key);
                putActionResult = cacheResult.Success;
            }
            catch (Exception ex)
            {
                m_Logger.LogInformation(string.Format(EXCEPTION_MESSAGE, EngineType, "PutStringAsync", LogUtils.GetExceptionDetails(ex)));
                putActionResult = false;
            }

            return putActionResult;
        }

        private void initializeParameters()
        {
            SSMParameter hostnameParameter = SSMClient.Instance.GetParameter(SSMLibrary.Constants.POD_ELASTICACHE_MEMCACHED_URL_PATH);
            HostName = hostnameParameter.Value;

            SSMParameter portParameter = SSMClient.Instance.GetParameter(SSMLibrary.Constants.POD_ELASTICACHE_MEMCACHED_PORT_PATH);
            Port = int.Parse(portParameter.Value);
        }
    }
}

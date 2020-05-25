using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TransactionLibrary.Abstraction;
using TransactionLibrary.Engines.Clients;
using TransactionLibrary.Enums;

namespace TransactionLibrary
{
    public sealed class ECClient
    {
        // Lazy thread safe singelton initiation.
        public static ECClient Instance { get { return sr_Instance.Value; } }
        private static readonly Lazy<ECClient> sr_Instance = new Lazy<ECClient>(() => new ECClient());

        // Retry configurations
        private const int DEFAULT_RETRY_COUNT = 5;
        private const double DELAY_BETWEEN_RETRIES_IN_SECONDS = 1.0;

        // EC keys format.
        private const string REQUEST_XML_KEY_FORMAT = "XML:{0}";
        private const string SF_TOKEN_KEY_FORMAT = "SF_token:{0}";
        private const string SF_DATA_KEY_FORMAT = "SF_Request_JSON:{0}";
        private const string SF_OUTPUT_KEY_FORMAT = "SF_Output_JSON:{0}";
        private const string SO_DATA_KEY_FORMAT = "{0}";
        private const string SO_ACK_KEY_FORMAT = "INT_ACK:{0}";
        private const string SO_RESPONSE_KEY_FORMAT = "INT_FIN:{0}";

        private IAsyncECClient m_ECClient = null;

        public string HostName => m_ECClient.HostName;
        public int Port => m_ECClient.Port;
        public eECEngineType EngineType => m_ECClient.EngineType;


        private ECClient()
        {
            eECEngineType ecEngine = eECEngineType.Redis;
            string ecTypeEnvironmentVariable = Environment.GetEnvironmentVariable("ECType");
            if (!string.IsNullOrEmpty(ecTypeEnvironmentVariable))
            {
                if (!Enum.TryParse(ecTypeEnvironmentVariable, ignoreCase: true, result: out ecEngine))
                {
                    ecEngine = eECEngineType.Redis;
                }
            }

            switch (ecEngine)
            {
                case eECEngineType.MemCached:
                    m_ECClient = new MemCachedClient();
                    break;
                case eECEngineType.Redis:
                    m_ECClient = new RedisClient();
                    break;
                default:
                    break;
            }
        }


        public async Task<ECOperationResult> GetRequestAsync(string i_TransactionId)
        {
            return await getStringAsyncHandler(i_TransactionId, REQUEST_XML_KEY_FORMAT).ConfigureAwait(false);
        }

        public async Task<ECOperationResult> GetSFTokenAsync(string i_TransactionId)
        {
            return await getStringAsyncHandler(i_TransactionId, SF_TOKEN_KEY_FORMAT).ConfigureAwait(false);
        }

        public async Task<ECOperationResult> GetSFDataAsync(string i_TransactionId)
        {
            return await getStringAsyncHandler(i_TransactionId, SF_DATA_KEY_FORMAT).ConfigureAwait(false);
        }

        public async Task<ECOperationResult> GetSFOutputAsync(string i_TransactionId)
        {
            return await getStringAsyncHandler(i_TransactionId, SF_OUTPUT_KEY_FORMAT).ConfigureAwait(false);
        }

        public async Task<ECOperationResult> GetSODataAsync(string i_TransactionId)
        {
            return await getStringAsyncHandler(i_TransactionId, SO_DATA_KEY_FORMAT).ConfigureAwait(false);
        }

        public async Task<ECOperationResult> GetIntAckAsync(string i_TransactionId, TimeSpan? i_RetryTTL = null)
        {
            return await getStringAsyncHandler(i_TransactionId, SO_ACK_KEY_FORMAT, i_RetryTTL).ConfigureAwait(false);
        }

        public async Task<ECOperationResult> GetIntResponseAsync(string i_TransactionId, TimeSpan? i_RetryTTL = null)
        {
            return await getStringAsyncHandler(i_TransactionId, SO_RESPONSE_KEY_FORMAT, i_RetryTTL).ConfigureAwait(false);
        }

        public async Task<ECOperationResult> PutRequestAsync(string i_TransactionId, string i_RequestXML)
        {
            return await putStringAsyncHandler(i_TransactionId, REQUEST_XML_KEY_FORMAT, i_RequestXML).ConfigureAwait(false);
        }

        public async Task<ECOperationResult> PutSFTokenAsync(string i_TransactionId, string i_SFToken, TimeSpan i_TTL)
        {
            return await putStringAsyncHandler(i_TransactionId, SF_TOKEN_KEY_FORMAT, i_SFToken, i_TTL).ConfigureAwait(false);
        }

        public async Task<ECOperationResult> PutSFDataAsync(string i_TransactionId, string i_JSONData)
        {
            return await putStringAsyncHandler(i_TransactionId, SF_DATA_KEY_FORMAT, i_JSONData).ConfigureAwait(false);
        }

        public async Task<ECOperationResult> PutSFOutputAsync(string i_TransactionId, string i_JSONData)
        {
            return await putStringAsyncHandler(i_TransactionId, SF_OUTPUT_KEY_FORMAT, i_JSONData).ConfigureAwait(false);
        }

        public async Task<ECOperationResult> PutSODataAsync(string i_TransactionId, string i_JSONData)
        {
            return await putStringAsyncHandler(i_TransactionId, SO_DATA_KEY_FORMAT, i_JSONData).ConfigureAwait(false);
        }

        private async Task<ECOperationResult> getStringAsyncHandler(string i_TransactionId, string i_Format, TimeSpan? i_RetryTTL = null)
        {
            if (string.IsNullOrEmpty(i_TransactionId))
            {
                throw new ArgumentNullException("i_TransactionId");
            }
            string key = string.Format(i_Format, i_TransactionId);
            return await getStringAsyncWithRetry(key, i_RetryTTL).ConfigureAwait(false);
        }

        private async Task<ECOperationResult> getStringAsyncWithRetry(string i_Key, TimeSpan? i_RetryTTL = null)
        {
            if (i_RetryTTL == null)
            {
                i_RetryTTL = TimeSpan.FromSeconds(DEFAULT_RETRY_COUNT * DELAY_BETWEEN_RETRIES_IN_SECONDS);
            }

            string valueFromEC = string.Empty;
            bool isSuccessful = false;
            int retryAttempt;
            Stopwatch elapsedTimeCounter = new Stopwatch();
            int numberOfRetries = Convert.ToInt32(Math.Ceiling(i_RetryTTL.Value.TotalSeconds / DELAY_BETWEEN_RETRIES_IN_SECONDS));
            elapsedTimeCounter.Start();

            for (retryAttempt = 1; retryAttempt <= numberOfRetries; retryAttempt++)
            {
                try
                {
                    valueFromEC = await m_ECClient.GetStringAsync(i_Key);
                }
                catch (Exception getException)
                {
                    valueFromEC = string.Empty;
                }

                if (!string.IsNullOrEmpty(valueFromEC))
                {
                    isSuccessful = true;
                    valueFromEC = Regex.Unescape(valueFromEC).Trim('"');
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(DELAY_BETWEEN_RETRIES_IN_SECONDS));
            }

            elapsedTimeCounter.Stop();

            return new ECOperationResult()
            {
                Key = i_Key,
                Success = isSuccessful,
                Elapsed = elapsedTimeCounter.Elapsed,
                RetryCount = retryAttempt,
                Value = valueFromEC
            };
        }

        private async Task<ECOperationResult> putStringAsyncHandler(string i_TransactionId, string i_Format, string i_Value, TimeSpan? i_TTL = null)
        {
            if (string.IsNullOrEmpty(i_TransactionId))
            {
                throw new ArgumentNullException("i_TransactionId");
            }

            if (string.IsNullOrEmpty(i_Value))
            {
                throw new ArgumentNullException("i_Value");
            }

            if (i_TTL == null)
            {
                i_TTL = TimeSpan.FromDays(1);
            }

            string key = string.Format(i_Format, i_TransactionId);
            return await putStringAsyncWithRetry(key, i_Value, i_TTL.Value).ConfigureAwait(false);
        }

        private async Task<ECOperationResult> putStringAsyncWithRetry(string i_Key, string i_Value, TimeSpan i_TTL)
        {
            bool isSuccessful = false;
            int retryAttempt;
            Stopwatch elapsedTimeCounter = new Stopwatch();
            elapsedTimeCounter.Start();

            for (retryAttempt = 1; retryAttempt <= DEFAULT_RETRY_COUNT; retryAttempt++)
            {
                try
                {
                    isSuccessful = await m_ECClient.PutStringAsync(i_Key, i_Value, i_TTL);
                }
                catch (Exception putException)
                {
                    isSuccessful = false;
                }

                if (isSuccessful)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(DELAY_BETWEEN_RETRIES_IN_SECONDS));
            }

            elapsedTimeCounter.Stop();

            return new ECOperationResult()
            {
                Key = i_Key,
                Success = isSuccessful,
                Elapsed = elapsedTimeCounter.Elapsed,
                RetryCount = retryAttempt
            };
        }

        public async Task DeleteDataAsync(string i_Key)
        {
            List<Task<bool>> deleteTasks = new List<Task<bool>>();

            deleteTasks.Add(m_ECClient.DeleteKeyAsync(string.Format(REQUEST_XML_KEY_FORMAT, i_Key)));
            deleteTasks.Add(m_ECClient.DeleteKeyAsync(string.Format(SF_DATA_KEY_FORMAT, i_Key)));
            deleteTasks.Add(m_ECClient.DeleteKeyAsync(string.Format(SF_OUTPUT_KEY_FORMAT, i_Key)));
            deleteTasks.Add(m_ECClient.DeleteKeyAsync(string.Format(SO_DATA_KEY_FORMAT, i_Key)));
            deleteTasks.Add(m_ECClient.DeleteKeyAsync(string.Format(SO_ACK_KEY_FORMAT, i_Key)));
            deleteTasks.Add(m_ECClient.DeleteKeyAsync(string.Format(SO_RESPONSE_KEY_FORMAT, i_Key)));

            Task.WaitAll(deleteTasks.ToArray());

            await Task.CompletedTask;
        }

        public async Task<bool> DeleteSFTokenAsync(string i_Key)
        {
            return await m_ECClient.DeleteKeyAsync(string.Format(SF_TOKEN_KEY_FORMAT, i_Key));
        }
    }
}

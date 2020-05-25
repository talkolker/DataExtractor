using System;

namespace SSMLibrary
{
    public static class Constants
    {
        public static readonly string FSL_SF_CLIENT_ID_PATH = "/FSL/SF/ClientId";
        public static readonly string FSL_SF_CLIENT_SECRET_PATH = "/FSL/SF/ClientSecret";
        public static readonly string FSL_FIREHOSE_REGION_PATH = "/FSL/Firehose/Region";
        public static readonly string FSL_FIREHOSE_STREAM_PATH = "/FSL/Firehose/Stream";

        private static readonly string FSL_RETRY_INTERVAL = "/Retry/Interval";
        private static readonly string FSL_RETRY_MAX_NUMBER_OF_TRIES = "/Retry/MaxNumberOfTries";

        public static readonly string POD_SQS_REGION_PATH;
        public static readonly string POD_SQS_IN_SHORT_PATH;
        public static readonly string POD_SQS_IN_LONG_PATH;
        public static readonly string POD_SQS_INSIGHTS_PATH;
        public static readonly string POD_ELASTICACHE_MEMCACHED_URL_PATH;
        public static readonly string POD_ELASTICACHE_MEMCACHED_PORT_PATH;
        public static readonly string POD_ELASTICACHE_REDIS_URL_PATH;
        public static readonly string POD_ELASTICACHE_REDIS_PORT_PATH;
        public static readonly string POD_RETRY_INTERVAL;
        public static readonly string POD_RETRY_MAX_NUMBER_OF_TRIES;
        public static readonly string POD_GIS_AUTH;

        private static readonly string POD_PATH;

        static Constants()
        {
            string podName = Environment.GetEnvironmentVariable("Pod");
            POD_PATH = string.IsNullOrEmpty(podName) ? "/FSL" : string.Format("/FSL/{0}", podName);
            POD_SQS_REGION_PATH = string.Format("{0}/SQS/Region", POD_PATH);
            POD_SQS_IN_SHORT_PATH = string.Format("{0}/SQS/In/Short", POD_PATH);
            POD_SQS_IN_LONG_PATH = string.Format("{0}/SQS/In/Long", POD_PATH); 
            POD_ELASTICACHE_MEMCACHED_URL_PATH = string.Format("{0}/Elasticache/Memcached/Url", POD_PATH);
            POD_ELASTICACHE_MEMCACHED_PORT_PATH = string.Format("{0}/Elasticache/Memcached/Port", POD_PATH);
            POD_ELASTICACHE_REDIS_URL_PATH = string.Format("{0}/Elasticache/Redis/Url", POD_PATH);
            POD_ELASTICACHE_REDIS_PORT_PATH = string.Format("{0}/Elasticache/Redis/Port", POD_PATH);
            POD_RETRY_INTERVAL = string.Format("{0}{1}", POD_PATH, FSL_RETRY_INTERVAL);
            POD_RETRY_MAX_NUMBER_OF_TRIES = string.Format("{0}{1}", POD_PATH, FSL_RETRY_MAX_NUMBER_OF_TRIES);
            POD_GIS_AUTH = string.Concat(POD_PATH, "/GIS/Auth");
            POD_SQS_INSIGHTS_PATH = string.Concat(POD_PATH, "/SQS/Insights");
        }
    }
}

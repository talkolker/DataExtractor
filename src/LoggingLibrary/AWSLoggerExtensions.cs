using System;
using Microsoft.Extensions.Logging;

namespace LoggingLibrary
{
    public static class AWSLoggerExtensions
    {
        private static string defaultContext = "No Context provided";
        private static string defaultMessage = "No Message provided";
        private static string defaultDetails = "No Details provided";
        private static AWSLogger m_AWSLogger;
        public static ILoggerFactory AddAWSLogger(this ILoggerFactory factory,
                                              AWSLogger AWSLogger,
                                              Func<string, LogLevel, bool> filter = null)
        {
            factory.AddProvider(new AWSLoggerProvider(filter, AWSLogger));
            return factory;
        }

        public static ILoggerFactory AddAWSLogger(this ILoggerFactory factory, AWSLogger AWSLogger, LogLevel minLevel)
        {
            m_AWSLogger = AWSLogger;
            return AddAWSLogger(
                factory,
                AWSLogger,
                (_, logLevel) => logLevel >= minLevel);
        }

        public static void Log(this ILogger logger, LogLevel i_LogLevel, string i_Context, string i_Message, string i_Details = "", int i_Count = 0, double i_DurationInSeconds = 0)
        {
            logger.Log(i_LogLevel, string.Format("Message: {0} - Details: {1}", i_Message, i_Details));
            logger.LogExtension(i_LogLevel, i_Context, i_Message, i_Details, i_Count, i_DurationInSeconds);
        }
        
        public static void LogInformation(this ILogger logger, string i_Context, string i_Message, string i_Details = "", int i_Count = 0, double i_DurationInSeconds = 0)
        {
            logger.LogInformation(string.Format("Message: {0} - Details: {1}", i_Message, i_Details));
            logger.LogExtension(LogLevel.Information, i_Context, i_Message, i_Details, i_Count, i_DurationInSeconds);
        }

        public static void LogError(this ILogger logger, string i_Context, string i_Message, string i_Details = "", int i_Count = 0, double i_DurationInSeconds = 0)
        {
            logger.LogError(string.Format("Message: {0} - Details: {1}", i_Message, i_Details));
            logger.LogExtension(LogLevel.Error, i_Context, i_Message, i_Details, i_Count, i_DurationInSeconds);
        }

        public static void LogWarning(this ILogger logger, string i_Context, string i_Message, string i_Details = "", int i_Count = 0, double i_DurationInSeconds = 0)
        {
            logger.LogWarning(string.Format("Message: {0} - Details: {1}", i_Message, i_Details));
            logger.LogExtension(LogLevel.Warning, i_Context, i_Message, i_Details, i_Count, i_DurationInSeconds);
        }

        public static void LogCritical(this ILogger logger, string i_Context, string i_Message, string i_Details = "", int i_Count = 0, double i_DurationInSeconds = 0)
        {
            logger.LogCritical(string.Format("Message: {0} - Details: {1}", i_Message, i_Details));
            logger.LogExtension(LogLevel.Critical, i_Context, i_Message, i_Details, i_Count, i_DurationInSeconds);
        }

        public static void LogExtension(this ILogger logger, LogLevel logLevel, string i_Context, string i_Message, string i_Details, int i_Count, double i_DurationInSeconds)
        {
            if (!logger.IsEnabled(logLevel))
                return;

            if (i_Context == "")
                i_Context = defaultContext;

            if (i_Message == "")
                i_Message = defaultMessage;

            if (i_Details == "")
                i_Details = defaultDetails;

            try
            {
                m_AWSLogger.AddDataToSend(i_Context, i_Message, i_Count, i_DurationInSeconds, i_Details, logLevel);

                m_AWSLogger.SendDataAsync();
            }
            catch (Exception ex)
            {
                logger.LogDebug("m_AWSLogger " + LogUtils.GetExceptionDetails(ex));
            }

        }
    }
}

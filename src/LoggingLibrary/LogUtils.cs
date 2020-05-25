using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace LoggingLibrary
{
    public static class LogUtils
    {
        public static ILoggerFactory LoggerFactory { get; } = new LoggerFactory();
        public static ILogger CreateLogger<T>() =>
          LoggerFactory.CreateLogger<T>();

        public static ILogger CreateLogger(string i_CategoryName) =>
            LoggerFactory.CreateLogger(i_CategoryName);

        public static string GetExceptionDetails(Exception i_ExceptionToLog)
        {
            return getExceptionDetails(i_ExceptionToLog, string.Empty);
        }

        private static string getExceptionDetails(Exception i_ExceptionToLog, string i_NewLinePrefix)
        {
            StringBuilder exceptionInfo = new StringBuilder();

            if (i_ExceptionToLog != null)
            {
                exceptionInfo.AppendLine(string.Format("{0}Exception: {1}", i_NewLinePrefix, i_ExceptionToLog.Message));
                exceptionInfo.AppendLine(getExceptionDetails(i_ExceptionToLog.InnerException, string.Concat(i_NewLinePrefix, "\t")));
                exceptionInfo.AppendLine(string.Format("{0}Stack: {1}", i_NewLinePrefix, i_ExceptionToLog.StackTrace));
            }

            return exceptionInfo.ToString();
        }
    }
}

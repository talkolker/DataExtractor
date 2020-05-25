using System;
using Microsoft.Extensions.Logging;

namespace LoggingLibrary
{
    public class AWSLoggerUtils : ILogger
    {
        private string _categoryName;
        private Func<string, LogLevel, bool> _filter;
        private AWSLogger _AWSLogger;

        public AWSLoggerUtils(string categoryName, Func<string, LogLevel, bool> filter, AWSLogger AWSLogger)
        {
            _categoryName = categoryName;
            _filter = filter;
            _AWSLogger = AWSLogger;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (_filter == null || _filter(_categoryName, logLevel));
        }


        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }



        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
            //throw new NotImplementedException();
        }
    }
}

using System;
using Microsoft.Extensions.Logging;

namespace LoggingLibrary
{
    public class AWSLoggerProvider : ILoggerProvider
    {
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly AWSLogger _AWSLogger;

        public AWSLoggerProvider(Func<string, LogLevel, bool> filter, AWSLogger AWSLogger)
        {
            _AWSLogger = AWSLogger;
            _filter = filter;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new AWSLoggerUtils(categoryName, _filter, _AWSLogger);
        }

        public void Dispose()
        {
        }
    }
}

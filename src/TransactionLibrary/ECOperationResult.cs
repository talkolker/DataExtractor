using System;

namespace TransactionLibrary
{
    public class ECOperationResult
    {
        public string Key { get; set; }
        public bool Success { get; set; }
        public int RetryCount { get; set; }
        public string Value { get; set; }
        public TimeSpan Elapsed { get; set; }
        public Exception Exception { get; set; }
    }
}

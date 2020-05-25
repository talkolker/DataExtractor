using System;
using Newtonsoft.Json;

namespace SalesforceLibrary.Exceptions
{
    [JsonObject]
    public class SFGeneralException : Exception
    {

        public string ErrorCode { get; }
        public SFGeneralException()
        {
        }

        public SFGeneralException(string message) :
            base(message)
        {
        }

        public SFGeneralException(string message, Exception innerException) :
            base(message, innerException)
        {
        }


        [JsonConstructor]
        private SFGeneralException(string message, string errorCode) :
            this(message)
        {
            ErrorCode = errorCode;
        }
    }
}

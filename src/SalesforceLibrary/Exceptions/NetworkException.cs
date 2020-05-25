using System;

namespace SalesforceLibrary.Exceptions
{
    public class NetworkException : Exception
    {
        public NetworkException(string message) : base(message)
        {
        }
    }
}

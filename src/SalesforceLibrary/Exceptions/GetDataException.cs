using System;

namespace SalesforceLibrary.Exceptions
{
    public class GetDataException : Exception
    {
        public string Details { get; }

        public GetDataException(string i_DataIdentifier, string i_ResponseContent) :
            this(i_DataIdentifier, i_ResponseContent, null)
        { }

        public GetDataException(string i_DataIdentifier, string i_ResponseContent, Exception innerException) :
            base(string.Format("Cannot get data for Optimization with id: {0}", i_DataIdentifier), innerException)
        {
            Details = i_ResponseContent;
        }
    }
}

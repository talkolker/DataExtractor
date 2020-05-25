using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionLibrary
{
    public class IntegrationResponse
    {
        public bool Success { get; set; }
        public string TransactionID { get; set; }
        public long PayloadSize { get; set; }
        public TimeSpan PayloadTimeToDownload { get; set; }
        public string Exception { get; set; }
    }
}

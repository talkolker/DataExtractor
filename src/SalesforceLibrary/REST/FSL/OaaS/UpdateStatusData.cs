using System;
using System.Collections.Generic;
using System.Text;

namespace SalesforceLibrary.REST.FSL.OaaS
{
    internal class UpdateStatusData
    {
        public string status { get; set; }

        public string dataID { get; set; }

        public string failReason { get; set; }

        public string failureDetails { get; set; }
    }
}

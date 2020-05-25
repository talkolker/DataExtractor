using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.DataModel.Utils;

namespace SalesforceLibrary.REST.FSL.OaaS
{
    class WriteResultsResult
    {
        public List<Metric> Metrics { get; set; }
    }
}

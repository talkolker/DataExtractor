using SalesforceLibrary.DataModel.Standard;
using System;
using System.Collections.Generic;
using System.Text;

namespace SalesforceLibrary.REST.FSL.OaaS
{
    internal class WriteResultsData
    {
        public string jsonStr { get; set; }

        public string dataID { get; set; }

        public int currIndex { get; set; }

        public bool isLast { get; set; }

        public int numberOfServicesToSchedule { get; set; }

        public int numberOfServicesScheduled { get; set; }

        public int avgTravelTimePriorToRSO { get; set; }

        public int avgTravelTimeAfterRSO { get; set; }

        public string resultsText { get; set; }
}
}

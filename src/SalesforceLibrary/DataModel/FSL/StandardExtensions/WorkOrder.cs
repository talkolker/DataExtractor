using Newtonsoft.Json;
using System;

namespace SalesforceLibrary.DataModel.Standard
{
    public partial class WorkOrder
    {
        [PackageNamespace("FSL")]
        public int? Scheduling_Priority__c
        {
            get
            {
                double? tempValue = NumericFields["FSL__Scheduling_Priority__c"];
                if (tempValue.HasValue)
                {
                    return Convert.ToInt32(tempValue.Value);
                }
                return null;
            }
            set { NumericFields["FSL__Scheduling_Priority__c"] = value; }
        }

        [PackageNamespace("FSL")]
        [JsonProperty]
        private string VisitingHours__c { get; set; }

        [PackageNamespace("FSL")]
        [JsonIgnore]
        public OperatingHours VisitingHours__r { get; set; }
    }
}

using Newtonsoft.Json;

namespace SalesforceLibrary.DataModel.Standard
{
    public partial class WorkOrderLineItem
    {
        //[PackageNamespace("FSL")]
        //[JsonProperty]
        //private string VisitingHours__c { get; set; }

        [PackageNamespace("FSL")]
        [JsonIgnore]
        public OperatingHours VisitingHours__r { get; set; }
    }
}

using Newtonsoft.Json;
using SalesforceLibrary.DataModel.FSL;
using System.Collections.Generic;

namespace SalesforceLibrary.DataModel.Standard
{
    public partial class ServiceAppointment
    {

        private Scheduling_Policy__c m_PolicyUsed = null;

        [PackageNamespace("FSL")]
        public bool? IsMultiDay__c
        {
            get { return BooleanFields["FSL__IsMultiDay__c"]; }
            set { BooleanFields["FSL__IsMultiDay__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Pinned__c
        {
            get { return BooleanFields["FSL__Pinned__c"]; }
            set { BooleanFields["FSL__Pinned__c"] = value; }
        }
        
        [PackageNamespace("FSL")]
        [JsonProperty]
        private string Related_Service__c { get; set; }

        [PackageNamespace("FSL")]
        [JsonIgnore]
        public ServiceAppointment Related_Service__r { get; set; }

        [PackageNamespace("FSL")]
        public bool? Same_Day__c
        {
            get { return BooleanFields["FSL__Same_Day__c"]; }
            set { BooleanFields["FSL__Same_Day__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Same_Resource__c
        {
            get { return BooleanFields["FSL__Same_Resource__c"]; }
            set { BooleanFields["FSL__Same_Resource__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public string Schedule_Mode__c { get; set; }

        [PackageNamespace("FSL")]
        [JsonProperty]
        private string Scheduling_Policy_Used__c { get; set; }

        [PackageNamespace("FSL")]
        [JsonIgnore]
        public Scheduling_Policy__c Scheduling_Policy_Used__r
        {
            get
            {
                return m_PolicyUsed;
            }
            set
            {
                m_PolicyUsed = value;
                Scheduling_Policy_Used__c = m_PolicyUsed?.Id;
            }
        }

        [PackageNamespace("FSL")]
        public string Time_Dependency__c { get; set; }

        [PackageNamespace("FSL")]
        public bool? UpdatedByOptimization__c
        {
            get { return BooleanFields["FSL__UpdatedByOptimization__c"]; }
            set { BooleanFields["FSL__UpdatedByOptimization__c"] = value; }
        }

        [JsonIgnore]
        public List<Time_Dependency__c> Time_Dependencies__r { get; set; }

        [JsonIgnore]
        public bool SkipAssignmentCreation { get; set; }
        [PackageNamespace("FSL")]
        public bool? InJeopardy__c
        {
            get { return BooleanFields["FSL__InJeopardy__c"]; }
            set { BooleanFields["FSL__InJeopardy__c"] = value; }
        }


        [PackageNamespace("FSL")]
        [JsonProperty]
        public string InJeopardyReason__c { get; set; }
        [JsonIgnore]
        public int Number { get; set; }
        
        [JsonIgnore]
        public bool isPinned
        {
            get { return Pinned__c.HasValue && Pinned__c.Value; }
        }

        [JsonIgnore]
        public bool ScheduledToInvalidResource { get; set; }

        [JsonIgnore]
        public ServiceTerritoryMember ScheduledSTM { get; set; }

        [JsonIgnore]
        public bool inMSTChain { get; set; }

        [JsonIgnore]
        public string RootCallId { get; set; }
    }
}

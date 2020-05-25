using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SalesforceLibrary.DataModel.FSL
{
    [JsonObject]
    public class OptimizationSettings__c : sObject
    {
        [PackageNamespace("FSL")]
        public double? Bulk_Update_Size__c
        {
            get { return NumericFields["FSL__Bulk_Update_Size__c"]; }
            set { NumericFields["FSL__Bulk_Update_Size__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Clear_Gantt__c
        {
            get { return BooleanFields["FSL__Clear_Gantt__c"]; }
            set { BooleanFields["FSL__Clear_Gantt__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public double? Cluster_Radius__c
        {
            get { return NumericFields["FSL__Cluster_Radius__c"]; }
            set { NumericFields["FSL__Cluster_Radius__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public double? Optimization_Data_Batch_Size__c
        {
            get { return NumericFields["FSL__Optimization_Data_Batch_Size__c"]; }
            set { NumericFields["FSL__Optimization_Data_Batch_Size__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public double? Maximum_Concurrent_Requests__c
        {
            get { return NumericFields["FSL__Maximum_Concurrent_Requests__c"]; }
            set { NumericFields["FSL__Maximum_Concurrent_Requests__c"] = value; }
        }

        [PackageNamespace("FSL")]
        [JsonProperty]
        private double? Max_Runtime_Optimization__c
        {
            get { return NumericFields["FSL__Max_Runtime_Optimization__c"]; }
            set { NumericFields["FSL__Max_Runtime_Optimization__c"] = value; }
        }

        [JsonIgnore]
        public TimeSpan? Max_Runtime_Optimization
        {
            get
            {
                TimeSpan? result = null;
                double? privateValue = Max_Runtime_Optimization__c;
                if (privateValue.HasValue)
                {
                    result = TimeSpan.FromSeconds(privateValue.Value);
                }
                return result;
            }
        }

        [PackageNamespace("FSL")]
        [JsonProperty]
        private string Pinned_Statuses__c { get; set; }

        [JsonIgnore]
        public ImmutableHashSet<string> PinnedStatuses { get; private set; }

        [PackageNamespace("FSL")]
        public double? Maximum_services_for_OaaS_in_24_hours__c
        {
            get { return NumericFields["FSL__Maximum_services_for_OaaS_in_24_hours__c"]; }
            set { NumericFields["FSL__Maximum_services_for_OaaS_in_24_hours__c"] = value; }
        }

        [PackageNamespace("FSL")]
        [JsonProperty]
        private double? Max_Grade__c
        {
            get { return NumericFields["FSL__Max_Grade__c"]; }
            set { NumericFields["FSL__Max_Grade__c"] = value; }
        }

        [JsonIgnore]
        public TimeSpan? Max_Grade
        {
            get
            {
                TimeSpan? result = null;
                double? privateValue = Max_Grade__c;
                if (privateValue.HasValue)
                {
                    result = TimeSpan.FromMinutes(privateValue.Value);
                }
                return result;
            }
        }

        [PackageNamespace("FSL")]
        public string Logic_Domain__c { get; set; }

        [PackageNamespace("FSL")]
        [JsonProperty]
        private double? Min_Grade__c
        {
            get { return NumericFields["FSL__Min_Grade__c"]; }
            set { NumericFields["FSL__Min_Grade__c"] = value; }
        }

        [JsonIgnore]
        public TimeSpan? Min_Grade
        {
            get
            {
                TimeSpan? result = null;
                double? privateValue = Min_Grade__c;
                if (privateValue.HasValue)
                {
                    result = TimeSpan.FromMinutes(privateValue.Value);
                }
                return result;
            }
        }

        [PackageNamespace("FSL")]
        [JsonProperty]
        private double? Max_Runtime_Single_Service__c
        {
            get { return NumericFields["FSL__Max_Runtime_Single_Service__c"]; }
            set { NumericFields["FSL__Max_Runtime_Single_Service__c"] = value; }
        }

        [JsonIgnore]
        public TimeSpan? Max_Runtime_Single_Service
        {
            get
            {
                TimeSpan? result = null;
                double? privateValue = Max_Runtime_Single_Service__c;
                if (privateValue.HasValue)
                {
                    result = TimeSpan.FromSeconds(privateValue.Value);
                }
                return result;
            }
        }

        [PackageNamespace("FSL")]
        public double? Reshuffle_Retries__c
        {
            get { return NumericFields["FSL__Reshuffle_Retries__c"]; }
            set { NumericFields["FSL__Reshuffle_Retries__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public string Optimizer__c { get; set; }

        [PackageNamespace("FSL")]
        public double? KD_Tree_Number_Of_Neighbors_To_Search__c
        {
            get { return NumericFields["FSL__KD_Tree_Number_Of_Neighbors_To_Search__c"]; }
            set { NumericFields["FSL__KD_Tree_Number_Of_Neighbors_To_Search__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public double? Cluster_Min_Points__c
        {
            get { return NumericFields["FSL__Cluster_Min_Points__c"]; }
            set { NumericFields["FSL__Cluster_Min_Points__c"] = value; }
        }
        [PackageNamespace("FSL")]
        public double? Radius__c
        {
            get { return NumericFields["FSL__Radius__c"]; }
            set { NumericFields["FSL__Radius__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Activate_Optimizer_Log__c
        {
            get { return BooleanFields["FSL__Activate_Optimizer_Log__c"]; }
            set { BooleanFields["FSL__Activate_Optimizer_Log__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Demands_Ordering__c
        {
            get { return BooleanFields["FSL__Demands_Ordering__c"]; }
            set { BooleanFields["FSL__Demands_Ordering__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Extended_Gap_Selector__c
        {
            get { return BooleanFields["FSL__Extended_Gap_Selector__c"]; }
            set { BooleanFields["FSL__Extended_Gap_Selector__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public string Extra_Categories__c
        {
            get { return StringFields["FSL__Extra_Categories__c"]; }
            set { StringFields["FSL__Extra_Categories__c"] = value; }
        }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            HashSet<string> pinnedStatusesSet;
            if (string.IsNullOrEmpty(Pinned_Statuses__c))
            {
                pinnedStatusesSet =  new HashSet<string>();
            } else
            {
                pinnedStatusesSet = Pinned_Statuses__c.Split(',').ToHashSet();
            }
            PinnedStatuses = pinnedStatusesSet.ToImmutableHashSet();

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}

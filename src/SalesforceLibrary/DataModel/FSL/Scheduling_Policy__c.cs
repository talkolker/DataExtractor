using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using System;
using System.Collections.Generic;

namespace SalesforceLibrary.DataModel.FSL
{
    [JsonObject]
    public class Scheduling_Policy__c : sObject
    {
        [PackageNamespace("FSL")]
        public string Description__c { get; set; }

        [PackageNamespace("FSL")]
        public int? Service_Clustering_Weight__c
        {
            get
            {
                double? tempValue = NumericFields["FSL__Service_Clustering_Weight__c"];
                if (tempValue.HasValue)
                {
                    return Convert.ToInt32(tempValue.Value);
                }
                return null;
            }
            set { NumericFields["FSL__Service_Clustering_Weight__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public int? Service_Priority_Weight__c
        {
            get
            {
                double? tempValue = NumericFields["FSL__Service_Priority_Weight__c"];
                if (tempValue.HasValue)
                {
                    return Convert.ToInt32(tempValue.Value);
                }
                return null;
            }
            set { NumericFields["FSL__Service_Priority_Weight__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Travel_Mode__c
        {
            get { return BooleanFields["FSL__Travel_Mode__c"]; }
            set { BooleanFields["FSL__Travel_Mode__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Fix_Overlaps__c
        {
            get { return BooleanFields["FSL__Fix_Overlaps__c"]; }
            set { BooleanFields["FSL__Fix_Overlaps__c"] = value; }
        }

        [JsonIgnore]
        public List<Scheduling_Policy_Goal__c> Objectives { get; }

        [JsonIgnore]
        public List<Work_Rule__c> Rules { get; set; }

        [PackageNamespace("FSL")]
        [JsonProperty("Scheduling_Policy_Goals")]
        private RelatedObjectCollection<Scheduling_Policy_Goal__c> scheduling_Policy_GoalsCollection { get; set; }

        [PackageNamespace("FSL")]
        [JsonProperty("Scheduling_Policy_Work_Rule")]
        private RelatedObjectCollection<Scheduling_Policy_Work_Rule__c> scheduling_Policy_Work_RuleCollection { get; set; }

        public Scheduling_Policy__c() :
            this(null)
        { }

        public Scheduling_Policy__c(string i_ObjectId) :
            base(i_ObjectId)
        {
            Objectives = new List<Scheduling_Policy_Goal__c>();
            Rules = new List<Work_Rule__c>();
        }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            DeserializationUtils.ProcessRelatedObjectsCollection(scheduling_Policy_GoalsCollection, i_ReferenceResolver);
            scheduling_Policy_GoalsCollection = null;

            DeserializationUtils.ProcessRelatedObjectsCollection(scheduling_Policy_Work_RuleCollection, i_ReferenceResolver);
            scheduling_Policy_Work_RuleCollection = null;

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}

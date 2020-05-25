using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using System;

namespace SalesforceLibrary.DataModel.FSL
{
    [JsonObject]
    public class Scheduling_Policy_Goal__c : sObject
    {
        [PackageNamespace("FSL")]
        [JsonProperty]
        private string Service_Goal__c { get; set; }

        [PackageNamespace("FSL")]
        [JsonIgnore]
        public Service_Goal__c Service_Goal__r { get; set; }

        [PackageNamespace("FSL")]
        public int? Weight__c
        {
            get
            {
                double? tempValue = NumericFields["FSL__Weight__c"];
                if (tempValue.HasValue)
                {
                    return Convert.ToInt32(tempValue.Value);
                }
                return null;
            }
            set { NumericFields["FSL__Weight__c"] = value; }
        }

        [PackageNamespace("FSL")]
        [JsonProperty]
        private string Scheduling_Policy__c { get; set; }

        [PackageNamespace("FSL")]
        [JsonIgnore]
        public Scheduling_Policy__c Scheduling_Policy__r { get; set; }

        public Scheduling_Policy_Goal__c() :
            this(null)
        { }

        public Scheduling_Policy_Goal__c(string i_ObjectId) :
            base(i_ObjectId)
        { }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            Type myType = GetType();

            string propertyName = "Scheduling_Policy__r";
            propertyName = DeserializationUtils.GetPropertyName(myType.GetProperty(propertyName), propertyName);
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary(propertyName);
            Scheduling_Policy__r = DeserializationUtils.GetSingleObjectReference<Scheduling_Policy__c>(Scheduling_Policy__c, objectJson, i_ReferenceResolver);
            if (Scheduling_Policy__r != null)
            {
                Scheduling_Policy__r.Objectives.Add(this);
            }

            propertyName = "Service_Goal__r";
            propertyName = DeserializationUtils.GetPropertyName(myType.GetProperty(propertyName), propertyName);
            objectJson = removeObjectJTokenFromAdditionalDictionary(propertyName);
            Service_Goal__r = DeserializationUtils.GetSingleObjectReference<Service_Goal__c>(Service_Goal__c, objectJson, i_ReferenceResolver);

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}

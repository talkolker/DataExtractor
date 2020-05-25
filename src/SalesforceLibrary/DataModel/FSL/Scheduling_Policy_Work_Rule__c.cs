using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using System;

namespace SalesforceLibrary.DataModel.FSL
{
    [JsonObject]
    public class Scheduling_Policy_Work_Rule__c : sObject
    {
        [PackageNamespace("FSL")]
        [JsonProperty]
        private string Scheduling_Policy__c { get; set; }

        [PackageNamespace("FSL")]
        [JsonIgnore]
        public Scheduling_Policy__c Scheduling_Policy__r { get; set; }

        [PackageNamespace("FSL")]
        [JsonProperty]
        private string Work_Rule__c { get; set; }

        [PackageNamespace("FSL")]
        [JsonIgnore]
        public Work_Rule__c Work_Rule__r { get; set; }

        public Scheduling_Policy_Work_Rule__c() :
            this(null)
        { }

        public Scheduling_Policy_Work_Rule__c(string i_ObjectId) :
            base(i_ObjectId)
        { }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            Type myType = GetType();

            string propertyName = "Work_Rule__r";
            propertyName = DeserializationUtils.GetPropertyName(myType.GetProperty(propertyName), propertyName);
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary(propertyName);
            Work_Rule__r = DeserializationUtils.GetSingleObjectReference<Work_Rule__c>(Work_Rule__c, objectJson, i_ReferenceResolver);


            propertyName = "Scheduling_Policy__r";
            propertyName = DeserializationUtils.GetPropertyName(myType.GetProperty(propertyName), propertyName);
            objectJson = removeObjectJTokenFromAdditionalDictionary(propertyName);
            Scheduling_Policy__r = DeserializationUtils.GetSingleObjectReference<Scheduling_Policy__c>(Scheduling_Policy__c, objectJson, i_ReferenceResolver);
            if (Scheduling_Policy__r != null && Work_Rule__r != null)
            {
                Scheduling_Policy__r.Rules.Add(Work_Rule__r);
            }

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}
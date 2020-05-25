using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.DataModel.Utils;
using System;
using System.Collections.Generic;

namespace SalesforceLibrary.DataModel.FSL
{
    [JsonObject]
    public class Time_Dependency__c : sObject
    {
        [PackageNamespace("FSL")]
        public string Root_Service_Appointment__c { get; set; }

        [JsonIgnore]
        public ServiceAppointment Root_Service_Appointment__r { get; set; }
        
        [PackageNamespace("FSL")]
        public string Dependency__c { get; set; }
        
        [PackageNamespace("FSL")]
        public bool Same_Resource__c { get; set; }
            
        [PackageNamespace("FSL")]
        public string Service_Appointment_1__c { get; set; }

        [JsonIgnore]
        public ServiceAppointment Service_Appointment_1__r { get; set; }

        [PackageNamespace("FSL")]
        public string Service_Appointment_2__c { get; set; }

        [JsonIgnore]
        public ServiceAppointment Service_Appointment_2__r { get; set; }

        public Time_Dependency__c(string i_ObjectId) :
            base(i_ObjectId)
        { }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            Type myType = GetType();

            string propertyName = "Root_Service_Appointment__r";
            propertyName = DeserializationUtils.GetPropertyName(myType.GetProperty(propertyName), propertyName);
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary(propertyName);
            Root_Service_Appointment__r = DeserializationUtils.GetSingleObjectReference<ServiceAppointment>(Root_Service_Appointment__c, objectJson, i_ReferenceResolver);
            

            propertyName = "Service_Appointment_1__r";
            propertyName = DeserializationUtils.GetPropertyName(myType.GetProperty(propertyName), propertyName);
            objectJson = removeObjectJTokenFromAdditionalDictionary(propertyName);
            Service_Appointment_1__r = DeserializationUtils.GetSingleObjectReference<ServiceAppointment>(Service_Appointment_1__c, objectJson, i_ReferenceResolver);
            if (Service_Appointment_1__r != null)
            {
                if (Service_Appointment_1__r.Time_Dependencies__r == null)
                {
                    Service_Appointment_1__r.Time_Dependencies__r = new List<Time_Dependency__c>();
                }

                Service_Appointment_1__r.Time_Dependencies__r.Add(this);
            }

            propertyName = "Service_Appointment_2__r";
            propertyName = DeserializationUtils.GetPropertyName(myType.GetProperty(propertyName), propertyName);
            objectJson = removeObjectJTokenFromAdditionalDictionary(propertyName);
            Service_Appointment_2__r = DeserializationUtils.GetSingleObjectReference<ServiceAppointment>(Service_Appointment_2__c, objectJson, i_ReferenceResolver);
//            if (Service_Appointment_2__r != null)
//            {
//                if (Service_Appointment_2__r.Time_Dependencies__r == null)
//                {
//                    Service_Appointment_2__r.Time_Dependencies__r = new List<Time_Dependency__c>();
//                }
//
//                Service_Appointment_2__r.Time_Dependencies__r.Add(this);
//            }

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Processor;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.Requests;

namespace SalesforceLibrary.DataModel.Utils.sObjectUtils
{
    public class ObjectiveUtils : sObjectUtils
    {
        private class DeserializedQueryResult
        {
            public Double? totalSize;
            public Boolean? done;
            public List<Scheduling_Policy_Goal__c> records;
        }
        
        public override void Deserialize(string i_QueryResult, AppointmentBookingData i_ABData,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            //TODO: add support for long responses that has to be pulled with identifier
            DeserializedQueryResult deserializedQuery =
                JsonConvert.DeserializeObject<DeserializedQueryResult>(i_QueryResult);

            i_ABData.ObjectivesByDevName = parseAdditionalData(deserializedQuery.records);
        }

        private Dictionary<string, List<Service_Goal__c>> parseAdditionalData(List<Scheduling_Policy_Goal__c> i_DeserializedQueryRecords)
        {
            List<Service_Goal__c> objectives = new List<Service_Goal__c>();
            foreach (Scheduling_Policy_Goal__c objectiveWrapper in i_DeserializedQueryRecords)
            {
                JToken objectiveToken = objectiveWrapper.m_JSONAdditionalData["Service_Goal__r"];
                Service_Goal__c objective = objectiveToken.ToObject<Service_Goal__c>(); ;
                objective.Weight__c = objectiveWrapper.Weight__c;
                objective.DeveloperName = objective.m_JSONAdditionalData["RecordType"].Last.ToObject<string>();
                objectives.Add(objective);
            }

            Dictionary<string, List<Service_Goal__c>> objectiveToReturn = new Dictionary<string, List<Service_Goal__c>>();
            foreach (Service_Goal__c objective in objectives)
            {
                if(!objectiveToReturn.ContainsKey(objective.DeveloperName))
                    objectiveToReturn[objective.DeveloperName] = new List<Service_Goal__c>();
                
                objectiveToReturn[objective.DeveloperName].Add(objective);
            }

            return objectiveToReturn;
        }

        public override string getQuery(AppointmentBookingRequest i_Request = null,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            string objectivesQuery = "SELECT Service_Goal__r.Ignore_Home_Base_Coordinates__c,Service_Goal__r.Resource_Priority_Field__c,"
            + "Service_Goal__r.Prioritize_Resource__c,Service_Goal__r.Object_Group_Field__c, Service_Goal__r.Resource_Group_Field__c," +
            "Weight__c, Service_Goal__r.RecordType.DeveloperName, Service_Goal__r.ID, Service_Goal__r.Name  FROM Scheduling_Policy_Goal__c " +
            $"WHERE Scheduling_Policy__c = '{i_Request.PolicyId}' " +
            "AND (NOT Service_Goal__r.RecordType.DeveloperName = 'Objective_Custom_Logic') order by Weight__c DESC";
            
            return formatQueryString(objectivesQuery);
        }
    }
}
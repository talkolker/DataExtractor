using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Processor;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.Requests;

namespace SalesforceLibrary.DataModel.Utils.sObjectUtils
{
    public class ObjectiveUtils : IObjectUtils
    {
        private class DeserializedQueryResult
        {
            public Double? totalSize;
            public Boolean? done;
            public List<Scheduling_Policy_Goal__c> records;
        }
        
        public override void Deserialize(string i_QueryResult, AppointmentBookingData i_ABData)
        {
            //TODO: add support for long responses that has to be pulled with identifier
            DeserializedQueryResult deserializedQuery =
                JsonConvert.DeserializeObject<DeserializedQueryResult>(i_QueryResult);

            i_ABData.Objectives = deserializedQuery.records;
        }

        public override string getQuery(AppointmentBookingRequest i_Request)
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
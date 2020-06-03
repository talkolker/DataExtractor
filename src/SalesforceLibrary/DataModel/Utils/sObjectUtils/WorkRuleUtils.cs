using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Processor;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.Requests;

namespace SalesforceLibrary.DataModel.Utils.sObjectUtils
{
    public class WorkRuleUtils : sObjectUtils
    {
        private static readonly List<List<string>> m_RulesReqFields;

        private class DeserializedQueryResult
        {
            public Double? totalSize;
            public Boolean? done;
            public List<Work_Rule__c> records;
        }
        
        static WorkRuleUtils()
        {
            List<string> TimeRuleFields = new List<string>(){"Pass_Empty_Values__c", "Service_Schedule_Time_Property__c", "Service_Time_Operator__c", "Service_Time_Property__c"};
            List<string> MatchFieldsRuleFields = new List<string>(){"Boolean_Operator__c","Service_Property__c"};
            List<string> GapRuleFields = new List<string>(){"Travel_From_Home__c", "Travel_To_Home__c"};
            List<string> MatchSkillsRuleFields = new List<string>() {"Match_Skill_Level__c"};
            List<string> MatchBooleanRuleFields = new List<string>(){"Match_Constant_Boolean_Value__c"};
            List<string> MatchCrewSizeRuleFields = new List<string>(){"Crew_Resources_Availability__c"};
            List<string> WorkingLocationsRuleFields = new List<string>(){"Working_Location_Enable_Primary__c"};
            List<string> CountRuleFields = new List<string>()
                {"CountType__c", "CountObject__c", "DefaultLimit__c", "CustomFieldName__c", "CountTimeResolution__c"};
            List<string> EnhancedMatchRuleFields = new List<string>(){"isTimephased__c", "WorkRule_Start_DateTime_Field__c",
                "WorkRule_End_DateTime_Field__c","Service_Appointment_Matching_Field__c", "Enhanced_Match_Linking_Object__c", "Service_Linking_Object_Reference_Field__c"};
            
            
            m_RulesReqFields = new List<List<string>>();
            m_RulesReqFields.Add(TimeRuleFields);
            m_RulesReqFields.Add(MatchFieldsRuleFields);
            m_RulesReqFields.Add(GapRuleFields);
            m_RulesReqFields.Add(MatchSkillsRuleFields);
            m_RulesReqFields.Add(MatchBooleanRuleFields);
            m_RulesReqFields.Add(MatchCrewSizeRuleFields);
            m_RulesReqFields.Add(WorkingLocationsRuleFields);
            m_RulesReqFields.Add(CountRuleFields);
            m_RulesReqFields.Add(EnhancedMatchRuleFields);
        }

        public override void Deserialize(string i_QueryResult, AppointmentBookingData i_ABData,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            //TODO: add support for long responses that has to be pulled with identifier
            DeserializedQueryResult deserializedQuery =
                JsonConvert.DeserializeObject<DeserializedQueryResult>(i_QueryResult);

            parseAdditionalData(deserializedQuery.records);
            
            i_ABData.RulesByDevName = new Dictionary<string, List<Work_Rule__c>>();
            foreach (Work_Rule__c workRule in deserializedQuery.records)
            {
                if(!i_ABData.RulesByDevName.ContainsKey(workRule.DeveloperName))
                    i_ABData.RulesByDevName[workRule.DeveloperName] = new List<Work_Rule__c>(); 
                
                i_ABData.RulesByDevName[workRule.DeveloperName].Add(workRule);
            }
        }
        
        protected void parseAdditionalData(List<Work_Rule__c> i_DeserializedObjects)
        {
            foreach (Work_Rule__c rule in i_DeserializedObjects)
            {
                JToken developerName = rule.m_JSONAdditionalData["RecordType"].Last;
                rule.DeveloperName = developerName.ToObject<string>();
            }
        }

        public override string getQuery(AppointmentBookingRequest i_Request = null,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            string rulesQuery =
                "SELECT Maximum_Travel_From_Home__c,Maximum_Travel_From_Home_Type__c,Id,Enable_Overtime__c,Name,Object_Group_Field__c, Resource_Property__c" +
                ",Resource_Group_Field__c, RecordType.DeveloperName ,Start_of_Day__c,Break_Start__c,Break_Duration__c,Is_Fixed_Gap__c,Minimum_Gap__c," +
                $" {addRRequiredRuleFields()} FROM Work_Rule__c " +
                $"WHERE Id in (SELECT Work_Rule__c FROM Scheduling_Policy_Work_Rule__c WHERE Scheduling_Policy__c = '{i_Request.PolicyId}')";

            return formatQueryString(rulesQuery);
        }
        
        private string addRRequiredRuleFields()
        {
            string requiredFields ="";
            foreach (List<string> fieldsPerRule in m_RulesReqFields)
            {
                string ruleReqFields = string.Join(" , ", fieldsPerRule);
                requiredFields = string.Concat(requiredFields, ruleReqFields + " , ");
            }

            requiredFields = requiredFields.Remove(requiredFields.Length - 3);
            return requiredFields;
        }
    }
}
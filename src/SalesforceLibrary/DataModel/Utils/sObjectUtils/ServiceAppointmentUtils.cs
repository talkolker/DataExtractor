using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Processor;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.Requests;

namespace SalesforceLibrary.DataModel.Utils.sObjectUtils
{
    public class ServiceAppointmentUtils : IObjectUtils
    {
        private Dictionary<string, List<Work_Rule__c>> m_RulesByDeveloperName;
        private class DeserializedQueryResult
        {
            public Double? totalSize;
            public Boolean? done;
            public List<ServiceAppointment> records;
        }
        
        private readonly Dictionary<string, List<string>> m_ServiceReqFieldsByRules;

        public ServiceAppointmentUtils(AppointmentBookingData i_ABData)
        {
            m_ServiceReqFieldsByRules = new Dictionary<string, List<string>>();
            m_RulesByDeveloperName = i_ABData.Rules;
            initializeRequiredFieldsFromRules(i_ABData);
        }

        private void initializeRequiredFieldsFromRules(AppointmentBookingData i_ABData)
        {
            //Time rule required fields
            List<string> timeRuleRequiredFields = new List<string>();
            if (i_ABData.Rules.TryGetValue("Time_Rule_Service", out var timeRules))
            {
                foreach (Work_Rule__c rule in timeRules)
                {
                    timeRuleRequiredFields.AddRange(new List<string>()
                        {rule.Service_Schedule_Time_Property__c, rule.Service_Time_Property__c});
                }

                m_ServiceReqFieldsByRules.Add("Time_Rule_Service", timeRuleRequiredFields);
            }

            //Match rule required fields
            List<string> matchFieldsRequiredFields = new List<string>();
            if (i_ABData.Rules.TryGetValue("Match_Fields_Service", out var matchFieldsRules))
            {
                foreach (Work_Rule__c rule in matchFieldsRules)
                {
                    matchFieldsRequiredFields.Add(rule.Service_Property__c);
                }

                m_ServiceReqFieldsByRules.Add("Match_Fields_Service", matchFieldsRequiredFields);
            }

            //Enhanced match rule required fields
            List<string> enhancedMatchRequiredFields = new List<string>();
            if (i_ABData.Rules.TryGetValue("Enhanced_Match_Service", out var enhancedMatchRules))
            {
                foreach (Work_Rule__c rule in enhancedMatchRules)
                {
                    enhancedMatchRequiredFields.Add(rule.Service_Appointment_Matching_Field__c);
                }

                m_ServiceReqFieldsByRules.Add("Enhanced_Match_Service", enhancedMatchRequiredFields);
            }

            //Count rule required fields
            List<string> countRuleRequiredFields = new List<string>();
            if (i_ABData.Rules.TryGetValue("Count_Rule", out var countRules))
            {
                foreach (Work_Rule__c rule in countRules)
                {
                    if (rule.CustomFieldName__c != null)
                        countRuleRequiredFields.Add(rule.CustomFieldName__c);
                }

                m_ServiceReqFieldsByRules.Add("Count_Rule", countRuleRequiredFields);
            }
            
            List<string> mdtFields = new List<string>(){i_ABData.LogicSettings.MDT_Boolean_Field__c};
            //Contractor MDS availability required fields
            if(i_ABData.Rules.ContainsKey("Contractor_MDS_Availability"))
                m_ServiceReqFieldsByRules.Add("Contractor_MDS_Availability", mdtFields);
            
            //Calendar availability required fields
            if(i_ABData.Rules.ContainsKey("Calendar_Availability_Service"))
                m_ServiceReqFieldsByRules.Add("Calendar_Availability_Service", mdtFields);
            
            //Availability secondary and shifts required fields
            if(i_ABData.Rules.ContainsKey("Availability_SecondaryAndShift"))
                m_ServiceReqFieldsByRules.Add("Availability_SecondaryAndShift", mdtFields);
        }

        public override void Deserialize(string i_QueryResult, AppointmentBookingData i_ABData)
        {
            DeserializedQueryResult deserializedQuery =
                JsonConvert.DeserializeObject<DeserializedQueryResult>(i_QueryResult);

            i_ABData.Services = deserializedQuery.records.ToDictionary(record => record.Id);
        }

        public override string getQuery(AppointmentBookingRequest i_Request)
        {
            string serviceIdStr = formatList(i_Request.ServiceIDs);
            string query = "SELECT id,status,related_service__c,same_day__c,same_resource__c,time_dependency__c,appointmentnumber"
                           +",duedate,earlieststarttime,schedstarttime,schedendtime,duration,durationtype,latitude,longitude," +
                           "internalslrgeolocation__latitude__s,internalslrgeolocation__longitude__s,serviceterritoryid," +
                           "schedule_over_lower_priority_appointment__c,use_async_logic__c,mdt_operational_time__c,ismultiday__c" +
                           ",parentrecordid,serviceterritory.operatinghours.timezone," +
                           "(SELECT estimated_travel_time_to_source__c," +
                           "estimated_travel_time_from_source__c,assignedresourcenumber,serviceresourceid,estimatedtraveltimefrom__c," +
                           "estimatedtraveltime,estimatedtraveldistancefrom__c,estimatedtraveldistanceto__c,serviceresource.servicecrewid," +
                           "serviceresource.resourcetype,serviceresource.iscapacitybased" +
                           " FROM serviceresources ORDER BY serviceresource.resourcetype desc nulls last, createddate asc nulls last)" +
                           $",(SELECT id FROM service_appointments__r),mds_calculated_length__c {addRRequiredRuleFields()} FROM ServiceAppointment WHERE id in {serviceIdStr}";

            return formatQueryString(query);
        }
        
        private string addRRequiredRuleFields()
        {
            string requiredFields ="";
            //foreach (string developerName in m_RulesByDeveloperName.Keys)
            //{
            //    List<string> fieldsPerRule = m_ServiceReqFieldsByRules[developerName];
            //    string ruleReqFields = string.Join(" , ", fieldsPerRule);
            //    requiredFields = string.Concat(requiredFields, ruleReqFields + " , ");
            //}
//
            //requiredFields = requiredFields.Remove(requiredFields.Length - 3);
            return requiredFields;
        }
    }
}
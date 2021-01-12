using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Processor;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.Requests;

namespace SalesforceLibrary.DataModel.Utils.sObjectUtils
{
    public class ServiceAppointmentUtils : sObjectUtils
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
            m_RulesByDeveloperName = i_ABData.RulesByDevName;
            initializeRequiredFieldsFromRules(i_ABData);
        }

        private void initializeRequiredFieldsFromRules(AppointmentBookingData i_ABData)
        {
            List<string> relevanceGroupFields = new List<string>();
            
            foreach (KeyValuePair<string, List<Work_Rule__c>> ruleByDev in i_ABData.RulesByDevName)
            {
                //Time rule required fields
                if (ruleByDev.Key.Equals("Time_Rule_Service"))
                {
                    List<string> timeRuleRequiredFields = new List<string>();
                    foreach (Work_Rule__c rule in ruleByDev.Value)
                    {
                        timeRuleRequiredFields.AddRange(new List<string>()
                            {rule.Service_Schedule_Time_Property__c, rule.Service_Time_Property__c});
                        
                        if(rule.Object_Group_Field__c != null)
                            relevanceGroupFields.Add(rule.Object_Group_Field__c);
                    }

                    if(!m_ServiceReqFieldsByRules.ContainsKey("Time_Rule_Service"))
                        m_ServiceReqFieldsByRules.Add("Time_Rule_Service", timeRuleRequiredFields);
                }

                //Match rule required fields
                else if (ruleByDev.Key.Equals("Match_Fields_Service"))
                {
                    List<string> matchFieldsRequiredFields = new List<string>();
                    foreach (Work_Rule__c rule in ruleByDev.Value)
                    {
                        matchFieldsRequiredFields.Add(rule.Service_Property__c);
                        if(rule.Object_Group_Field__c != null)
                            relevanceGroupFields.Add(rule.Object_Group_Field__c);
                    }

                    if(!m_ServiceReqFieldsByRules.ContainsKey("Match_Fields_Service"))
                        m_ServiceReqFieldsByRules.Add("Match_Fields_Service", matchFieldsRequiredFields);
                }

                //Enhanced match rule required fields
                else if (ruleByDev.Key.Equals("Enhanced_Match_Service"))
                {
                    List<string> enhancedMatchRequiredFields = new List<string>();
                    foreach (Work_Rule__c rule in ruleByDev.Value)
                    {
                        enhancedMatchRequiredFields.Add(rule.Service_Appointment_Matching_Field__c);
                        if(rule.Object_Group_Field__c != null)
                            relevanceGroupFields.Add(rule.Object_Group_Field__c);
                    }

                    if(!m_ServiceReqFieldsByRules.ContainsKey("Enhanced_Match_Service"))
                        m_ServiceReqFieldsByRules.Add("Enhanced_Match_Service", enhancedMatchRequiredFields);
                }

                //Count rule required fields
                else if (ruleByDev.Key.Equals("Count_Rule"))
                {
                    List<string> countRuleRequiredFields = new List<string>();
                    foreach (Work_Rule__c rule in ruleByDev.Value)
                    {
                        if (rule.CustomFieldName__c != null)
                            countRuleRequiredFields.Add(rule.CustomFieldName__c);
                        if(rule.Object_Group_Field__c != null)
                            relevanceGroupFields.Add(rule.Object_Group_Field__c);
                    }

                    if(!m_ServiceReqFieldsByRules.ContainsKey("Count_Rule"))
                        m_ServiceReqFieldsByRules.Add("Count_Rule", countRuleRequiredFields);
                }

                else
                {
                    foreach (Work_Rule__c rule in ruleByDev.Value)
                    {
                        if(rule.Object_Group_Field__c != null)
                            relevanceGroupFields.Add(rule.Object_Group_Field__c);
                    }
                }
            }
            //For custom field in relevance group
            if(!m_ServiceReqFieldsByRules.ContainsKey("Relevance Group Fields"))
                m_ServiceReqFieldsByRules.Add("Relevance Group Fields", relevanceGroupFields);
        }

        public override void Deserialize(string i_QueryResult, AppointmentBookingData i_ABData,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default, bool async = false)
        {
            DeserializedQueryResult deserializedQuery =
                JsonConvert.DeserializeObject<DeserializedQueryResult>(i_QueryResult);

            i_ABData.ServicesById = parseAdditionalData(deserializedQuery.records);
        }
        
        internal Dictionary<string, ServiceAppointment> parseAdditionalData(List<ServiceAppointment> i_DeserializedQueryRecords)
        {
            Dictionary<string, ServiceAppointment> services = new Dictionary<string, ServiceAppointment>();
            foreach (ServiceAppointment service in i_DeserializedQueryRecords)
            {
                ServiceTerritory territory = null;
                OperatingHours opHours = null;
                List<AssignedResource> resources = null;
                
                if (service.m_JSONAdditionalData.TryGetValue("ServiceTerritory", out var territoryToken))
                {
                    territory = territoryToken.ToObject<ServiceTerritory>();
                    if (territory != null)
                    {
                        JToken opHoursToken = territory.m_JSONAdditionalData["OperatingHours"];
                        opHours = opHoursToken.ToObject<OperatingHours>();
                        territory.Id = service.ServiceTerritoryId;
                        territory.OperatingHours = opHours;
                    }
                }

                if (service.m_JSONAdditionalData.TryGetValue("ServiceResources", out var resourcesToken))
                {
                    resources = resourcesToken.ToObject<List<AssignedResource>>();
                }

                service.ServiceResources = resources;
                service.ServiceTerritory = territory;
                services.Add(service.Id, service);
            }
            
            return services;
        }

        public override string getQuery(AppointmentBookingRequest i_Request = null,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            string serviceIdStr = formatIdList(new List<string>(){i_Request.ServiceID});
            string query = "SELECT id,status,related_service__c,same_day__c,same_resource__c,time_dependency__c,appointmentnumber"
                           +",duedate,earlieststarttime,schedstarttime,schedendtime,duration,durationtype,latitude,longitude," +
                           "internalslrgeolocation__latitude__s,internalslrgeolocation__longitude__s,serviceterritoryid," +
                           "schedule_over_lower_priority_appointment__c,use_async_logic__c,mdt_operational_time__c,ismultiday__c" +
                           ",parentrecordid, parentrecordtype,serviceterritory.operatinghours.timezone," +
                           "(SELECT estimated_travel_time_to_source__c," +
                           "estimated_travel_time_from_source__c,assignedresourcenumber,serviceresourceid,estimatedtraveltimefrom__c," +
                           "estimatedtraveltime,estimatedtraveldistancefrom__c,estimatedtraveldistanceto__c,serviceresource.servicecrewid," +
                           "serviceresource.resourcetype,serviceresource.iscapacitybased" +
                           " FROM serviceresources ORDER BY serviceresource.resourcetype desc nulls last, createddate asc nulls last)" +
                           ",(SELECT id FROM service_appointments__r),mds_calculated_length__c {0}" +
                           $" FROM ServiceAppointment WHERE id in ({serviceIdStr})";

                /*
            query =
                "select id,status,related_service__c,same_day__c,same_resource__c,time_dependency__c,appointmentnumber,duedate" +
                ",earlieststarttime,schedstarttime,schedendtime,duration,durationtype,latitude,longitude," +
                "internalslrgeolocation__latitude__s,internalslrgeolocation__longitude__s,serviceterritoryid," +
                "schedule_over_lower_priority_appointment__c,use_async_logic__c,mdt_operational_time__c," +
                "ismultiday__c,parentrecordid,serviceterritory.operatinghours.timezone," +
                "(select estimated_travel_time_to_source__c,estimated_travel_time_from_source__c,assignedresourcenumber," +
                "serviceresourceid,estimatedtraveltimefrom__c,estimatedtraveltime,estimatedtraveldistancefrom__c," +
                "estimatedtraveldistanceto__c,serviceresource.servicecrewid,serviceresource.resourcetype,serviceresource.iscapacitybased " +
                "from serviceresources order by serviceresource.resourcetype desc nulls last, createddate asc nulls last)" +
                ",(select id from service_appointments__r),arrivalwindowstarttime,arrivalwindowendtime," +
                $"mds_calculated_length__c from ServiceAppointment where id in ({serviceIdStr})";
                */

            string requiredRuleFields = addRRequiredRuleFields(query);
            query = string.Format(query, requiredRuleFields);
            return formatQueryString(query);
        }
        
        private string addRRequiredRuleFields(string i_Query)
        {
            string requiredFields ="";
            foreach (string developerName in m_RulesByDeveloperName.Keys)
            {
                if (m_ServiceReqFieldsByRules.TryGetValue(developerName, out var fieldsPerRule))
                {
                    foreach (string ruleField in fieldsPerRule.Where(field => !i_Query.Contains(field, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        requiredFields +=  " , " + ruleField;
                    }
                }
            }
            
            if(m_ServiceReqFieldsByRules.TryGetValue("Relevance Group Fields", out var relevanceGroupFields))
            {
                if(relevanceGroupFields != null && relevanceGroupFields.Count > 0)
                    requiredFields += ", " + formatList(relevanceGroupFields);
            }

            return requiredFields;
        }

        public static bool getBooleanField(ServiceAppointment i_Service, string i_FieldName)
        {
            bool fieldValue = false;
            if (i_Service.m_JSONAdditionalData.TryGetValue(i_FieldName, out var value))
            {
                fieldValue = value.ToObject<bool>();
            }

            return fieldValue;
        }
    }
}
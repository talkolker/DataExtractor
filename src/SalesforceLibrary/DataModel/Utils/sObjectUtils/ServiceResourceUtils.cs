using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Processor;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.Requests;

namespace SalesforceLibrary.DataModel.Utils.sObjectUtils
{
    public class ServiceResourceUtils : sObjectUtils
    {
        private AppointmentBookingData m_ABData;
        private Dictionary<string, List<string>> m_ReqRuleFields;
        private Dictionary<string, List<string>> m_ReqObjectiveFields;
        
        private class DeserializedQueryResult
        {
            public Double? totalSize;
            public Boolean? done;
            public List<ServiceResource> records;
        }

        public ServiceResourceUtils(AppointmentBookingData i_ABData)
        {
            m_ABData = i_ABData;
            m_ReqObjectiveFields = new Dictionary<string, List<string>>();
            m_ReqRuleFields = new Dictionary<string, List<string>>();
            
            HashSet<string> Objective_Resource_Priority = new HashSet<string>();
            if (m_ABData.ObjectivesByDevName.TryGetValue("Objective_Resource_Priority", out var resPriorityObjs))
            {
                foreach (Service_Goal__c obj in resPriorityObjs)
                {
                    if(!string.IsNullOrEmpty(obj.Resource_Priority_Field__c))
                        Objective_Resource_Priority.Add(obj.Resource_Priority_Field__c);
                }
            }

            List<string> Skill_Level = new List<string>()
            {
                "(Select Id,SkillId,ServiceResourceId,SkillLevel,EffectiveStartDate,EffectiveEndDate " +
                "From ServiceResourceSkills Where (EffectiveStartDate <= {0}) AND " +
                "(EffectiveEndDate = null OR EffectiveEndDate >= {1}) AND SkillId in ({2}) " +
                "ORDER BY EffectiveStartDate ASC NULLS FIRST)"
            };
            
            m_ReqObjectiveFields.Add("Objective_Resource_Priority", Objective_Resource_Priority.ToList());
            m_ReqObjectiveFields.Add("Objective_Skill_Level", Skill_Level);
            
            HashSet<string> Count_Rule = new HashSet<string>();
            if (m_ABData.RulesByDevName.TryGetValue("Count_Rule", out var countRules))
            {
                foreach (Work_Rule__c rule in countRules)
                {
                    if(!string.IsNullOrEmpty(rule.Resource_Property__c))
                        Count_Rule.Add(rule.Resource_Property__c);
                }
            }
            
            HashSet<string> Match_Fields_Service = new HashSet<string>();
            if (m_ABData.RulesByDevName.TryGetValue("Match_Fields_Service", out var matchFieldsRules))
            {
                foreach (Work_Rule__c rule in matchFieldsRules)
                {
                    if(!string.IsNullOrEmpty(rule.Resource_Property__c))
                        Match_Fields_Service.Add(rule.Resource_Property__c);
                }
            }
            
            HashSet<string> Match_Boolean_Service = new HashSet<string>();
            if (m_ABData.RulesByDevName.TryGetValue("Match_Boolean_Service", out var matchBooleanRules))
            {
                foreach (Work_Rule__c rule in matchBooleanRules)
                {
                    if(!string.IsNullOrEmpty(rule.Resource_Property__c))
                        Match_Boolean_Service.Add(rule.Resource_Property__c);
                }
            }
            
            m_ReqRuleFields.Add("Match_Boolean_Service", Match_Boolean_Service.ToList());
            m_ReqRuleFields.Add("Match_Fields_Service", Match_Fields_Service.ToList());
            m_ReqRuleFields.Add("Count_Rule", Count_Rule.ToList());
            m_ReqRuleFields.Add("Match_Crew_Size_Service", new List<string>(){"ServiceCrew.CrewSize"});
            m_ReqRuleFields.Add("Match_Skills_Service", Skill_Level);
            //TODO: add fields for Enhanced match
        }
        public override void Deserialize(string i_QueryResult, AppointmentBookingData i_ABData,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            DeserializedQueryResult deserializedQuery =
                JsonConvert.DeserializeObject<DeserializedQueryResult>(i_QueryResult);

            bool serviceHasSkills = false;
            Dictionary<string, double?> skillIdToLevel = new Dictionary<string, double?>();
            foreach (SkillRequirement skillReq in i_ABData.ServiceParent[i_ABData.ServiceToSchedule.ParentRecordId].SkillRequirements)
            {
                skillIdToLevel.Add(skillReq.SkillId, skillReq.SkillLevel);
                serviceHasSkills = true;
            }

            bool matchSkillLevel = false;
            if(i_ABData.RulesByDevName.ContainsKey("Match_Skills_Service"))
            {
                List<Work_Rule__c> matchSkillsRules = i_ABData.RulesByDevName["Match_Skills_Service"];
                foreach (Work_Rule__c rule in matchSkillsRules)
                {
                    if (rule.Match_Skill_Level__c == true)
                        matchSkillLevel = true;
                }
            }
            
            i_ABData.CandidatesById = new Dictionary<string, ServiceResource>();
            if(!matchSkillLevel || !serviceHasSkills)
                i_ABData.CandidatesById = deserializedQuery.records.ToDictionary(candidate => candidate.Id);
            else
            {
                foreach (ServiceResource resource in deserializedQuery.records)
                {
                    if(resource.serviceResourceSkillsCollection == null)
                        continue;
                    
                    foreach (ServiceResourceSkill skill in resource.serviceResourceSkillsCollection.records)
                    {
                        if (skillIdToLevel.ContainsKey(skill.SkillId))
                        {
                            if (matchSkillLevel && skill.SkillLevel < skillIdToLevel[skill.SkillId])
                                continue;

                            if(!i_ABData.CandidatesById.ContainsKey(resource.Id))
                                i_ABData.CandidatesById.Add(resource.Id, resource);
                        }
                    }
                }
            }
        }

        public override string getQuery(AppointmentBookingRequest i_Request = null,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            ServiceAppointment service = m_ABData.ServiceToSchedule;
            DateTime minDate = service.EarliestStartTime.Value.AddDays(-7);
            DateTime maxDate = service.DueDate.Value.AddDays(7);
            string resourcesIds = formatIdList(m_ABData.STMResourcesIDs.ToList());
            string query = "select Id, name, IsActive, IsCapacityBased, Efficiency__c, Travel_Speed__c, "+
                           "RelatedRecordId, ResourceType, ServiceCrewId, (select ServiceResourceId," +
                           "StartDate,EndDate,ServiceCrewId from ServiceCrewMembers "+
                           $"where ((StartDate <= {formatDate(maxDate)}) AND (EndDate = null OR EndDate >= {formatDate(minDate)}))) {addRequiredFields(minDate, maxDate)}" +
                           $" from ServiceResource Where IsActive = true And Id in ({resourcesIds})";
            
            return formatQueryString(query);
        }

        private string addRequiredFields(DateTime i_MinDate, DateTime i_MaxDate)
        {
            ServiceAppointment service = m_ABData.ServiceToSchedule;
            HashSet<string> requiredSkillsIds = new HashSet<string>();
            ServiceParent serviceParent = m_ABData.ServiceParent[service.ParentRecordId];
            foreach (SkillRequirement skillReq in serviceParent.SkillRequirements) {
                requiredSkillsIds.Add(skillReq.SkillId);
            }
            
            HashSet<string> relevantSecondariesToRetrieve = new HashSet<string>();
            if (service.ServiceTerritory.Id != null) {
                relevantSecondariesToRetrieve.Add(service.ServiceTerritory.Id);
            }

            return getFields(requiredSkillsIds, relevantSecondariesToRetrieve, i_MinDate, i_MaxDate);
        }

        private string getFields(HashSet<string> i_RequiredSkillsIds, HashSet<string> i_RelevantSecondariesToRetrieve, DateTime i_MinDate, DateTime i_MaxDate)
        {
            string requiredFields = "";
            List<string> relevanceFields = new List<string>();
            foreach (KeyValuePair<string, List<Work_Rule__c>> rulesDevName in m_ABData.RulesByDevName)
            {
                if (m_ReqRuleFields.TryGetValue(rulesDevName.Key, out var fieldsByDev))
                {
                    foreach (string field in fieldsByDev.Where(field => !requiredFields.Contains(field)))
                    {
                        if (rulesDevName.Key.Equals("Match_Skills_Service"))
                        {
                            requiredFields += ", " + string.Format(field, formatDate(i_MaxDate), formatDate(i_MinDate),
                                formatIdList(i_RequiredSkillsIds.ToList()));
                        }
                        else
                            requiredFields += ", " + field; 
                    }
                }

                foreach (Work_Rule__c rule in rulesDevName.Value)
                {
                    if (!string.IsNullOrEmpty(rule.Resource_Group_Field__c))
                        relevanceFields.Add(rule.Resource_Group_Field__c);
                }
            }
            
            foreach (KeyValuePair<string, List<Service_Goal__c>> objDevName in m_ABData.ObjectivesByDevName)
            {
                if (m_ReqRuleFields.TryGetValue(objDevName.Key, out var fieldsByDev))
                {
                    foreach (string field in fieldsByDev.Where(field => !requiredFields.Contains(field)))
                    {
                        if (objDevName.Key.Equals("Objective_Skill_Level"))
                        {
                            requiredFields += ", " + string.Format(field, formatDate(i_MaxDate), formatDate(i_MinDate),
                                formatIdList(i_RequiredSkillsIds.ToList()));
                        }
                        else
                            requiredFields += ", " + field; 
                    }
                }

                foreach (Service_Goal__c obj in objDevName.Value)
                {
                    if (!string.IsNullOrEmpty(obj.Resource_Group_Field__c))
                        relevanceFields.Add(obj.Resource_Group_Field__c);
                }
            }

            string resTerrQuery;
            if (relevanceFields.Any())
            {
                resTerrQuery = "(Select ServiceTerritory.Internal_SLR_Geolocation__c,Internal_SLR_HomeAddress_Geolocation__c"+
                                      ",Id,ServiceResourceId,ServiceTerritory.Internal_SLR_Geolocation__Latitude__s," +
                                      "ServiceTerritory.Internal_SLR_Geolocation__Longitude__s,ServiceTerritory.Longitude" +
                                      ",ServiceTerritory.Latitude,ServiceTerritory.OperatingHours.TimeZone,OperatingHoursId," +
                                      "OperatingHours.TimeZone,EffectiveEndDate,EffectiveStartDate,Latitude,Longitude," +
                                      "Internal_SLR_HomeAddress_Geolocation__Latitude__s," +
                                      "Internal_SLR_HomeAddress_Geolocation__Longitude__s,TerritoryType,ServiceTerritoryId";
                foreach(string field in relevanceFields) {
                    resTerrQuery += ", " + field;
                }

                resTerrQuery += $" From ServiceTerritories Where EffectiveStartDate <= {formatDate(i_MaxDate)} AND "+
                                $"(EffectiveEndDate = null OR EffectiveEndDate >= {formatDate(i_MinDate)}) AND " +
                                "((TerritoryType = \'R\' OR TerritoryType = \'P\') OR " +
                                $"(ServiceTerritoryId in ({formatList(i_RelevantSecondariesToRetrieve.ToList())}) AND TerritoryType = \'S\')))";

            }
            else
            {
                resTerrQuery = "(Select ServiceTerritory.Internal_SLR_Geolocation__c,Internal_SLR_HomeAddress_Geolocation__c"+
                               ",Id,ServiceResourceId,ServiceTerritory.Internal_SLR_Geolocation__Latitude__s," +
                               "ServiceTerritory.Internal_SLR_Geolocation__Longitude__s,ServiceTerritory.Longitude," +
                               "ServiceTerritory.Latitude,ServiceTerritory.OperatingHours.TimeZone,OperatingHoursId," +
                               "OperatingHours.TimeZone,EffectiveEndDate,EffectiveStartDate,Latitude,Longitude," +
                               "Internal_SLR_HomeAddress_Geolocation__Latitude__s,Internal_SLR_HomeAddress_Geolocation__Longitude__s," +
                               $"TerritoryType,ServiceTerritoryId From ServiceTerritories Where EffectiveStartDate <= {formatDate(i_MaxDate)} " +
                               $"AND (EffectiveEndDate = null OR EffectiveEndDate >= {formatDate(i_MinDate)}) AND " +
                               "((TerritoryType = \'R\' OR TerritoryType = \'P\') OR " +
                               $"(ServiceTerritoryId in ({formatIdList(i_RelevantSecondariesToRetrieve.ToList())}) AND TerritoryType = \'S\')))";
            }

            requiredFields += ", " + resTerrQuery;
            return requiredFields;
        }
    }
}
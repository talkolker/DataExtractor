using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Processor;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.Requests;

namespace SalesforceLibrary.DataModel.Utils.sObjectUtils
{
    public class ServiceParentUtils : sObjectUtils
    {
        private Dictionary<string, string>  m_WOandWOLIParentsReqRuleFields;
        private Dictionary<string, string> m_WOParentsReqRuleFields;
        private Dictionary<string, string> m_WOandWOLIParentsReqObjectiveFields;
        private Dictionary<string, string> m_WOParentsReqObjectiveFields;
        private AppointmentBookingData m_ABData;

        public ServiceParentUtils(AppointmentBookingData i_ABData)
        {
            m_ABData = i_ABData;
            m_WOandWOLIParentsReqRuleFields = new Dictionary<string, string>();
            m_WOParentsReqRuleFields = new Dictionary<string, string>();
            m_WOandWOLIParentsReqObjectiveFields = new Dictionary<string, string>();
            m_WOParentsReqObjectiveFields = new Dictionary<string, string>();

            string Service_Visiting_Hours = "VisitingHours__c";
            string Required_Resources_Service = "(Select Id, ServiceResourceId, PreferenceType, RelatedRecordId From ResourcePreferences)";
            string Match_Skills_Service = "(Select Skill.MasterLabel,Id, RelatedRecordId,SkillLevel,SkillId From SkillRequirements)";
            string Excluded_Resource_Service = "(Select Id, ServiceResourceId, PreferenceType, RelatedRecordId From ResourcePreferences)";
            string Objective_Skill_Level = "(Select Skill.MasterLabel,Id, RelatedRecordId,SkillLevel,SkillId From SkillRequirements)";
            string Objective_PreferredEngineer = "(Select Id, ServiceResourceId, PreferenceType, RelatedRecordId From ResourcePreferences)";
            
            m_WOandWOLIParentsReqRuleFields.Add("Service_Visiting_Hours", Service_Visiting_Hours);
            m_WOandWOLIParentsReqRuleFields.Add("Match_Skills_Service", Match_Skills_Service);

            m_WOandWOLIParentsReqObjectiveFields.Add("Objective_Skill_Level", Objective_Skill_Level);
            
            m_WOParentsReqRuleFields.Add("Required_Resources_Service", Required_Resources_Service);
            m_WOParentsReqRuleFields.Add("Excluded_Resource_Service", Excluded_Resource_Service);
            
            m_WOParentsReqObjectiveFields.Add("Objective_PreferredEngineer", Objective_PreferredEngineer);
        }
        private class DeserializedQueryResult
        {
            public Double? totalSize;
            public Boolean? done;
            public List<ServiceParent> records;
        }
        
        public override void Deserialize(string i_QueryResult, AppointmentBookingData i_ABData,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default, bool async = false)
        {
            //TODO: implement this generic for WOLI and Account parents also
            DeserializedQueryResult deserializedQuery =
                JsonConvert.DeserializeObject<DeserializedQueryResult>(i_QueryResult);

            foreach (ServiceParent parent in deserializedQuery.records)
            {
                if (parent.SkillRequirements == null)
                    parent.SkillRequirements = new List<SkillRequirement>();

                foreach (SkillRequirement skillReq in parent.skillRequirementsCollection.records)
                {
                    parent.SkillRequirements.Add(skillReq);
                }
            }

            i_ABData.ServiceParent = deserializedQuery.records.ToDictionary(parent => parent.Id);
        }

        public override string getQuery(AppointmentBookingRequest i_Request = null,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            //TODO: WO priority and WOLI priority fields need to be queried. Field names are in the Logic settings
            string query = $"select id, MinimumCrewSize, scheduling_priority__c {addRequiredFiledsByRulesAndObjectives()} from WorkOrder where Id = '{m_ABData.ServiceToSchedule.ParentRecordId}'";

            query = formatQueryString(query);
            return query;
        }

        private string addRequiredFiledsByRulesAndObjectives()
        {
            string requiredFields = "";
            foreach (string rulesDevName in m_ABData.RulesByDevName.Keys)
            {
                if (m_WOParentsReqRuleFields.ContainsKey(rulesDevName) && !requiredFields.Contains(m_WOParentsReqRuleFields[rulesDevName]))
                {
                    requiredFields += ", " + m_WOParentsReqRuleFields[rulesDevName];
                }
                
                if (m_WOandWOLIParentsReqRuleFields.ContainsKey(rulesDevName) && !requiredFields.Contains(m_WOandWOLIParentsReqRuleFields[rulesDevName]))
                {
                    requiredFields += ", " + m_WOandWOLIParentsReqRuleFields[rulesDevName];
                }
            }
            
            foreach (string objectiveDevName in m_ABData.ObjectivesByDevName.Keys)
            {
                if (m_WOParentsReqObjectiveFields.ContainsKey(objectiveDevName) && !requiredFields.Contains(m_WOParentsReqObjectiveFields[objectiveDevName]))
                {
                    requiredFields += ", " + m_WOParentsReqObjectiveFields[objectiveDevName];
                }
                
                if (m_WOandWOLIParentsReqObjectiveFields.ContainsKey(objectiveDevName) && !requiredFields.Contains(m_WOandWOLIParentsReqObjectiveFields[objectiveDevName]))
                {
                    requiredFields += ", " + m_WOandWOLIParentsReqObjectiveFields[objectiveDevName];
                }
            }

            if (m_ABData.RulesByDevName.TryGetValue("Count_Rule", out var countRules))
            {
                List<string> count_Rule = new List<string>();
                foreach (Work_Rule__c countRule in countRules)
                {
                    if (!countRule.CountObject__c.Equals("ServiceAppointment") &&
                        m_ABData.ServiceToSchedule.ParentRecordType.Equals(countRule.CountObject__c))
                        count_Rule.Add(countRule.CustomFieldName__c);
                }

                requiredFields += ", " + formatList(count_Rule);
            }

            return requiredFields;
        }
    }
}
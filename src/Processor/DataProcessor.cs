using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SalesforceLibrary.DataModel;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.DataModel.Utils;
using SalesforceLibrary.DataModel.Utils.sObjectUtils;
using SalesforceLibrary.Requests;
using SalesforceLibrary.REST.FSL;

namespace Processor
{
    public class DataProcessor
    {
        private FSLClient m_FSLClient;
        private AppointmentBookingRequest m_Request;
        private AppointmentBookingData m_ABData;
        
        public DataProcessor(FSLClient i_FSLClient, SFDCScheduleRequest i_Request)
        {
            m_FSLClient = i_FSLClient;
            m_Request = i_Request as AppointmentBookingRequest;
            m_ABData = new AppointmentBookingData(m_Request);
        }
        
        public void ExtractData()
        {
            getPolicyRulesAndObjectives();
            getService();
        }

        private void getService()
        {
            IObjectUtils serviceUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(ServiceAppointment), m_ABData);
            string rulesQuery = serviceUtils.getQuery(m_Request);
            string objectivesStr = m_FSLClient.ExecuteQuery(rulesQuery);

            deselializeQueryResult(serviceUtils, objectivesStr);
        }

        private void getPolicyRulesAndObjectives()
        {
            //TODO: run parallel
            m_ABData.Rules = getRulesByPolicy();
            m_ABData.Objectives = getObjectivesByPolicy();
        }

        private List<Scheduling_Policy_Goal__c> getObjectivesByPolicy()
        {
            IObjectUtils objectivesUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(Scheduling_Policy_Goal__c));
            string objectivesQuery = objectivesUtils.getQuery(m_Request);
            string objectivesStr = m_FSLClient.ExecuteQuery(objectivesQuery);

            deselializeQueryResult(objectivesUtils, objectivesStr);
            return m_ABData.Objectives;
        }

        private Dictionary<string, List<Work_Rule__c>> getRulesByPolicy()
        {
            IObjectUtils rulesUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(Work_Rule__c));
            getWorkRulesObjectsByPolicy(rulesUtils);
            return getRulesClasses();
        }

        private void getWorkRulesObjectsByPolicy(IObjectUtils i_ObjectUtils)
        {
            string rulesQuery = i_ObjectUtils.getQuery(m_Request);
            string rulesStr = m_FSLClient.ExecuteQuery(rulesQuery);

            deselializeQueryResult(i_ObjectUtils, rulesStr);

            List<String> recordTypesToAdd = new List<String>();
            recordTypesToAdd.Add("Rule_MST_Service");
            recordTypesToAdd.Add("Match_Resource_Service");
            recordTypesToAdd.Add("Same_Day_Service");
            recordTypesToAdd.Add("Crew_Member_Availability_Service");
            recordTypesToAdd.Add("Contractor_Availability");
            recordTypesToAdd.Add("Contractor_MDS_Availability");
            recordTypesToAdd.Add("Validation_MDS_Duration");
        }

        private void deselializeQueryResult(IObjectUtils i_ObjectUtils, string i_QueryResultStr)
        {
            i_ObjectUtils.Deserialize(i_QueryResultStr, m_ABData); 
        }

        private Dictionary<string, List<Work_Rule__c>> getRulesClasses()
        {
            HashSet<String> columnsSet = new HashSet<String>();
            List<string> rulesToAddForSingleGapRule = new List<string>();
            
            if (!m_Request.IsEmergency) {
                rulesToAddForSingleGapRule.Add("Resource_Availability_Service");
            }
            
            rulesToAddForSingleGapRule.Add("Non_Availability_Service");
            rulesToAddForSingleGapRule.Add("Calendar_Availability_Service");
            rulesToAddForSingleGapRule.Add("Availability_SecondaryAndShift");

            //foreach (var VARIABLE in COLLECTION)
            {
                
            }

            return m_ABData.Rules;
        }
    }
}
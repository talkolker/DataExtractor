using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesforceLibrary.DataModel;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.DataModel.Utils;
using SalesforceLibrary.DataModel.Utils.sObjectUtils;
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
            getTimeDependencies();
            getMSTServices();
            //TODO: here is performed a query to bring the Dictionary for SA duration fields.
            //TODO: Since this is a custom setting it cannot be queried. This must be done in MP
            getSTMs();
            getServiceParentInformation();
            getVisitingHours();
            getCandidates();
            getUnlicensedUsers();
            getAdditionalObjects();
        }

        private async void getAdditionalObjects()
        {
            sObjectUtils additionalObjectsUtils = ObjectUtilsFactory.CreateUtilsByType(null, m_ABData, "AdditionalObjects");

            List<Task> tasks = new List<Task>()
            {
                Task.Run(() => getAdditionalServices(additionalObjectsUtils)),
                Task.Run(() => getAbsencesAndShifts(additionalObjectsUtils)),
                Task.Run(() => getResTerritories(additionalObjectsUtils)), 
                Task.Run(() => getCapacities(additionalObjectsUtils))
            };

            Task.WaitAll(tasks.ToArray());
            await Task.CompletedTask;

            //Dependant on resTerritoriesQuery
            string calendarsQuery = additionalObjectsUtils.getQuery(m_Request, AdditionalObjectsUtils.eAdditionalObjectQuery.Calendars);
            string calendarsStr = m_FSLClient.ExecuteQuery(calendarsQuery);
            deselializeQueryResult(additionalObjectsUtils, calendarsStr, AdditionalObjectsUtils.eAdditionalObjectQuery.Calendars);
        }

        private Task getCapacities(sObjectUtils additionalObjectsUtils)
        {
            string capacitiesQuery = additionalObjectsUtils.getQuery(m_Request, AdditionalObjectsUtils.eAdditionalObjectQuery.Capacities);
            string capacitiesStr = m_FSLClient.ExecuteQuery(capacitiesQuery);
            deselializeQueryResult(additionalObjectsUtils, capacitiesStr, AdditionalObjectsUtils.eAdditionalObjectQuery.Capacities);
            
            return Task.CompletedTask;
        }

        private Task getResTerritories(sObjectUtils additionalObjectsUtils)
        {
            string resTerritoriesQuery = additionalObjectsUtils.getQuery(m_Request, AdditionalObjectsUtils.eAdditionalObjectQuery.ResourceTerritories);
            string resTerritoriesStr = m_FSLClient.ExecuteQuery(resTerritoriesQuery);
            deselializeQueryResult(additionalObjectsUtils, resTerritoriesStr, AdditionalObjectsUtils.eAdditionalObjectQuery.ResourceTerritories);
            
            return Task.CompletedTask;
        }

        private Task getAbsencesAndShifts(sObjectUtils additionalObjectsUtils)
        {
            string absencesAndShiftsQuery = additionalObjectsUtils.getQuery(m_Request, AdditionalObjectsUtils.eAdditionalObjectQuery.ResourcesAdditionalObjects);
            string absencesAndShiftsStr = m_FSLClient.ExecuteQuery(absencesAndShiftsQuery);
            deselializeQueryResult(additionalObjectsUtils, absencesAndShiftsStr, AdditionalObjectsUtils.eAdditionalObjectQuery.ResourcesAdditionalObjects);
            
            return Task.CompletedTask;
        }

        private Task getAdditionalServices(sObjectUtils additionalObjectsUtils)
        {
            string resServicesQuery = additionalObjectsUtils.getQuery(m_Request, AdditionalObjectsUtils.eAdditionalObjectQuery.ServicesInResourcesTimeDomain);
            string resServicesStr = m_FSLClient.ExecuteQuery(resServicesQuery);
            deselializeQueryResult(additionalObjectsUtils, resServicesStr,AdditionalObjectsUtils.eAdditionalObjectQuery.ServicesInResourcesTimeDomain );
            
            string resServicesNextRecordsStr = m_FSLClient.ExecuteQuery(m_ABData.ABAdditionalObjects.nextRecordsUrl, true);
            deselializeQueryResult(additionalObjectsUtils, resServicesNextRecordsStr,AdditionalObjectsUtils.eAdditionalObjectQuery.ServicesInResourcesTimeDomain );
            
            return Task.CompletedTask;
        }

        private void getUnlicensedUsers()
        {
            sObjectUtils LicenseUtils = ObjectUtilsFactory.CreateUtilsByType(null, m_ABData, "UserLicense");
            string licenseQuery = LicenseUtils.getQuery(m_Request);
            string licenseStr = m_FSLClient.ExecuteQuery(licenseQuery);
            
            deselializeQueryResult(LicenseUtils, licenseStr);
        }

        private void getCandidates()
        {
            sObjectUtils resourceUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(ServiceResource), m_ABData);
            string resourceQuery = resourceUtils.getQuery(m_Request);
            string resourceStr = m_FSLClient.ExecuteQuery(resourceQuery);
            
            deselializeQueryResult(resourceUtils, resourceStr);
        }

        private void getVisitingHours()
        {
            sObjectUtils visitingHoursUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(OperatingHours), m_ABData);
            string visitingHoursQuery = visitingHoursUtils.getQuery(m_Request);
            if (visitingHoursQuery == String.Empty)
                return;
            
            string visitingHoursStr = m_FSLClient.ExecuteQuery(visitingHoursQuery);
            
            deselializeQueryResult(visitingHoursUtils, visitingHoursStr);
        }

        private void getServiceParentInformation()
        {
            string parentType = m_ABData.ServiceToSchedule.ParentRecordType;
            sObjectUtils parentUtils = ObjectUtilsFactory.CreateUtilsByType(null, m_ABData, parentType);
            string parentInfoQuery = parentUtils.getQuery(m_Request);
            string parentInfoStr = m_FSLClient.ExecuteQuery(parentInfoQuery);
            
            deselializeQueryResult(parentUtils, parentInfoStr);
        }

        private void getSTMs()
        {
            sObjectUtils STMServiceUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(ServiceTerritoryMember), m_ABData);
            string stmQuery = STMServiceUtils.getQuery(m_Request);
            string stmStr = m_FSLClient.ExecuteQuery(stmQuery);
            
            deselializeQueryResult(STMServiceUtils, stmStr);
            
            string stmAdditionalMembersQuery = STMServiceUtils.getQuery(m_Request);
            string stmAdditionalMembersQueryStr = m_FSLClient.ExecuteQuery(stmAdditionalMembersQuery);
            
            deselializeQueryResult(STMServiceUtils, stmAdditionalMembersQueryStr);
        }

        private void getMSTServices()
        {
            if (m_ABData.TimeDependeciesByRootId == null || !m_ABData.TimeDependeciesByRootId.Any())
                return;

            sObjectUtils MSTServiceUtils = ObjectUtilsFactory.CreateUtilsByType(null, m_ABData,"MSTServiceUtils");
            string serviceDependencyQuery = MSTServiceUtils.getQuery(m_Request);
            string serviceDependencyStr = m_FSLClient.ExecuteQuery(serviceDependencyQuery);
            
            deselializeQueryResult(MSTServiceUtils, serviceDependencyStr);
        }

        private void getTimeDependencies()
        {
            sObjectUtils timeDependenciesUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(Time_Dependency__c));
            string serviceDependencyQuery = timeDependenciesUtils.getQuery(m_Request);
            if (string.IsNullOrEmpty(serviceDependencyQuery))
                return;

            string serviceDependencyStr = m_FSLClient.ExecuteQuery(serviceDependencyQuery);
            
            deselializeQueryResult(timeDependenciesUtils, serviceDependencyStr);
            
            string rootDependenciesQuery = timeDependenciesUtils.getQuery(m_Request);
            if (string.IsNullOrEmpty(rootDependenciesQuery))
                return;
            
            string rootDependenciesStr = m_FSLClient.ExecuteQuery(rootDependenciesQuery);
            
            deselializeQueryResult(timeDependenciesUtils, rootDependenciesStr);
        }

        private void getService()
        {
            sObjectUtils serviceUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(ServiceAppointment), m_ABData);
            string serviceQuery = serviceUtils.getQuery(m_Request);
            string serviceStr = m_FSLClient.ExecuteQuery(serviceQuery);

            deselializeQueryResult(serviceUtils, serviceStr);
            m_ABData.ServiceToSchedule = m_ABData.ServicesById[m_Request.ServiceID];
        }

        private async void getPolicyRulesAndObjectives()
        {
            List<Task> tasks = new List<Task>()
            {
                Task.Run(getRulesByPolicy),
                Task.Run(getObjectivesByPolicy)
            };

            Task.WaitAll(tasks.ToArray());

            await Task.CompletedTask;
        }

        private Task getObjectivesByPolicy()
        {
            sObjectUtils objectivesUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(Scheduling_Policy_Goal__c));
            string objectivesQuery = objectivesUtils.getQuery(m_Request);
            string objectivesStr = m_FSLClient.ExecuteQuery(objectivesQuery);

            deselializeQueryResult(objectivesUtils, objectivesStr);

            return Task.CompletedTask;
        }

        private Task getRulesByPolicy()
        {
            sObjectUtils rulesUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(Work_Rule__c));
            getWorkRulesObjectsByPolicy(rulesUtils);
            getGapRuleRelatedRules();
            
            return Task.CompletedTask;
        }

        private void getWorkRulesObjectsByPolicy(sObjectUtils i_ObjectUtils)
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

            foreach (string ruleDeveloperName in recordTypesToAdd)
            {
                Work_Rule__c rule = new Work_Rule__c();
                rule.DeveloperName = ruleDeveloperName;

                if (!m_ABData.RulesByDevName.ContainsKey(ruleDeveloperName))
                    m_ABData.RulesByDevName[ruleDeveloperName] = new List<Work_Rule__c>();
                
                m_ABData.RulesByDevName[ruleDeveloperName].Add(rule);
            }
        }

        private void deselializeQueryResult(sObjectUtils i_ObjectUtils, string i_QueryResultStr, AdditionalObjectsUtils.eAdditionalObjectQuery i_ObjectToDeserialize = default)
        {
            i_ObjectUtils.Deserialize(i_QueryResultStr, m_ABData, i_ObjectToDeserialize); 
        }

        private void getGapRuleRelatedRules()
        {
            List<string> rulesToAddForSingleGapRule = new List<string>();
            
            if (!m_Request.IsEmergency) {
                rulesToAddForSingleGapRule.Add("Resource_Availability_Service");
            }
            
            rulesToAddForSingleGapRule.Add("Non_Availability_Service");
            rulesToAddForSingleGapRule.Add("Calendar_Availability_Service");
            rulesToAddForSingleGapRule.Add("Availability_SecondaryAndShift");

            if (m_ABData.RulesByDevName.TryGetValue("Gap_Rule_Service", out var gapRules))
            {
                foreach (Work_Rule__c gapRule in gapRules)
                {
                    foreach (string ruleName in rulesToAddForSingleGapRule)
                    {
                        if (!m_ABData.RulesByDevName.ContainsKey(ruleName))
                            m_ABData.RulesByDevName[ruleName] = new List<Work_Rule__c>();

                        m_ABData.RulesByDevName[ruleName].Add(gapRule);
                    }
                }
            }
        }
    }
}
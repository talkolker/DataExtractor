using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp.Extensions;
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
        private RestAPIMeasurments measurments;
        Stopwatch watch = new Stopwatch();
        
        public DataProcessor(FSLClient i_FSLClient, SFDCScheduleRequest i_Request)
        {
            m_FSLClient = i_FSLClient;
            m_Request = i_Request as AppointmentBookingRequest;
            m_ABData = new AppointmentBookingData(m_Request);
        }
        
        public void ExtractData(RestAPIMeasurments i_measurments)
        {
            measurments = i_measurments;
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

        private void getAdditionalObjects()
        {
            sObjectUtils additionalObjectsUtils = ObjectUtilsFactory.CreateUtilsByType(null, m_ABData, "AdditionalObjects");

            //Stopwatch syncWatch = new Stopwatch();
            //syncWatch.Start();
            //getAdditionalObjectsSync(additionalObjectsUtils);
            //syncWatch.Stop();
            //measurments.addMeasureToSubstarct(syncWatch.ElapsedMilliseconds);
            getAdditionalObjectsASync(additionalObjectsUtils);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            //Dependant on resTerritoriesQuery
            string calendarsQuery = additionalObjectsUtils.getQuery(m_Request, AdditionalObjectsUtils.eAdditionalObjectQuery.Calendars);
            string calendarsStr = m_FSLClient.ExecuteQuery(calendarsQuery, Measures.CALENDARS_QUERY, measurments);
            deselializeQueryResult(additionalObjectsUtils, calendarsStr, AdditionalObjectsUtils.eAdditionalObjectQuery.Calendars);
            
            watch.Stop();
           
            measurments.addMeasure(Measures.CALENDARS_PROCESSING, watch.ElapsedMilliseconds);
            watch.Reset();
        }
        private void getAdditionalObjectsSync(sObjectUtils additionalObjectsUtils)
        {
            getAdditionalServices(additionalObjectsUtils);
            getAbsencesAndShifts(additionalObjectsUtils);
            getResTerritories(additionalObjectsUtils);
            getCapacities(additionalObjectsUtils);
        }

        private async Task getAdditionalObjectsASync(sObjectUtils additionalObjectsUtils)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            List<Task> tasks = new List<Task>()
            {
                Task.Run(() => getAdditionalServices(additionalObjectsUtils, true)),
                Task.Run(() => getAbsencesAndShifts(additionalObjectsUtils, true)),
                Task.Run(() => getResTerritories(additionalObjectsUtils, true)), 
                Task.Run(() => getCapacities(additionalObjectsUtils, true))
            };

            Task.WaitAll(tasks.ToArray());
            await Task.CompletedTask;

            watch.Stop();
            
            measurments.addMeasure(Measures.ADITTIONAL_DATA_PARALLEL, watch.ElapsedMilliseconds);
            watch.Reset();
        }

        private Task getCapacities(sObjectUtils additionalObjectsUtils, bool async = false)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            string capacitiesQuery = additionalObjectsUtils.getQuery(m_Request, AdditionalObjectsUtils.eAdditionalObjectQuery.Capacities);
            string capacitiesStr = m_FSLClient.ExecuteQuery(capacitiesQuery, Measures.CAPACITIES_QUERY, measurments, false, async);
            deselializeQueryResult(additionalObjectsUtils, capacitiesStr, AdditionalObjectsUtils.eAdditionalObjectQuery.Capacities);
            
            watch.Stop();
            
            if(!async)
                measurments.addMeasure(Measures.CAPACITIES_PROCESSING, watch.ElapsedMilliseconds);
            
            watch.Reset();
            
            return Task.CompletedTask;
        }

        private Task getResTerritories(sObjectUtils additionalObjectsUtils, bool async = false)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            string resTerritoriesQuery = additionalObjectsUtils.getQuery(m_Request, AdditionalObjectsUtils.eAdditionalObjectQuery.ResourceTerritories);
            string resTerritoriesStr = m_FSLClient.ExecuteQuery(resTerritoriesQuery, Measures.ADITTIONAL_DATA_STM_QUERY, measurments,false, async);
            deselializeQueryResult(additionalObjectsUtils, resTerritoriesStr, AdditionalObjectsUtils.eAdditionalObjectQuery.ResourceTerritories);
            
            watch.Stop();
            
            if(!async)
                measurments.addMeasure(Measures.ADITTIONAL_DATA_STM_PROCESSING, watch.ElapsedMilliseconds);
            
            watch.Reset();
            
            return Task.CompletedTask;
        }

        private Task getAbsencesAndShifts(sObjectUtils additionalObjectsUtils, bool async = false)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            string absencesAndShiftsQuery = additionalObjectsUtils.getQuery(m_Request, AdditionalObjectsUtils.eAdditionalObjectQuery.ResourcesAdditionalObjects);
            string absencesAndShiftsStr = m_FSLClient.ExecuteQuery(absencesAndShiftsQuery, Measures.ABSENCES_SHIFTS_QUERY, measurments, false, async);
            deselializeQueryResult(additionalObjectsUtils, absencesAndShiftsStr, AdditionalObjectsUtils.eAdditionalObjectQuery.ResourcesAdditionalObjects);
            
            watch.Stop();
           
            if(!async)
                measurments.addMeasure(Measures.ABSENCES_SHIFTS_PROCESSING, watch.ElapsedMilliseconds);
            
            watch.Reset();
            
            return Task.CompletedTask;
        }

        private Task getAdditionalServices(sObjectUtils additionalObjectsUtils, bool async = false)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            string resServicesQuery = additionalObjectsUtils.getQuery(m_Request, AdditionalObjectsUtils.eAdditionalObjectQuery.ServicesInResourcesTimeDomain);
            string resServicesStr = m_FSLClient.ExecuteQuery(resServicesQuery, Measures.SAS_QUERY, measurments, false, async);
            deselializeQueryResult(additionalObjectsUtils, resServicesStr,AdditionalObjectsUtils.eAdditionalObjectQuery.ServicesInResourcesTimeDomain, async);
            
            //string resServicesNextRecordsStr = m_FSLClient.ExecuteQuery(m_ABData.ABAdditionalObjects.nextRecordsUrl, Measures.SAS_QUERY, measurments,true, async);
            //deselializeQueryResult(additionalObjectsUtils, resServicesNextRecordsStr,AdditionalObjectsUtils.eAdditionalObjectQuery.ServicesInResourcesTimeDomain, async);
            
            watch.Stop();
            
            if(!async)
                measurments.addMeasure(Measures.SAS_PROCESSING, watch.ElapsedMilliseconds);
            
            watch.Reset();
            
            return Task.CompletedTask;
        }

        private void getUnlicensedUsers()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            sObjectUtils LicenseUtils = ObjectUtilsFactory.CreateUtilsByType(null, m_ABData, "UserLicense");
            string licenseQuery = LicenseUtils.getQuery(m_Request);
            string licenseStr = m_FSLClient.ExecuteQuery(licenseQuery, Measures.UNLICENSED_USERS_QUERY, measurments);
            
            deselializeQueryResult(LicenseUtils, licenseStr);
            
            watch.Stop();
            
            measurments.addMeasure(Measures.UNLICENSED_USERS_PROCESSING, watch.ElapsedMilliseconds);
            watch.Reset();
        }

        private void getCandidates()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            sObjectUtils resourceUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(ServiceResource), m_ABData);
            string resourceQuery = resourceUtils.getQuery(m_Request);
            string resourceStr = m_FSLClient.ExecuteQuery(resourceQuery, Measures.RESOURCES_QUERY, measurments);
            
            deselializeQueryResult(resourceUtils, resourceStr);
            watch.Stop();
            
            measurments.addMeasure(Measures.RESOURCES_PROCESSING, watch.ElapsedMilliseconds);
            watch.Reset();
        }

        private void getVisitingHours()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            sObjectUtils visitingHoursUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(OperatingHours), m_ABData);
            string visitingHoursQuery = visitingHoursUtils.getQuery(m_Request);
            if (visitingHoursQuery == String.Empty)
            {
                watch.Stop();
                measurments.addMeasure(Measures.VISITING_HOURS_PROCESSING, watch.ElapsedMilliseconds);
                watch.Reset();
                return;
            }

            string visitingHoursStr = m_FSLClient.ExecuteQuery(visitingHoursQuery, Measures.VISITING_HOURS_QUERY, measurments);
            
            deselializeQueryResult(visitingHoursUtils, visitingHoursStr);
            watch.Stop();
            measurments.addMeasure(Measures.VISITING_HOURS_PROCESSING, watch.ElapsedMilliseconds);
            watch.Reset();
        }

        private void getServiceParentInformation()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            string parentType = m_ABData.ServiceToSchedule.ParentRecordType;
            sObjectUtils parentUtils = ObjectUtilsFactory.CreateUtilsByType(null, m_ABData, parentType);
            string parentInfoQuery = parentUtils.getQuery(m_Request);
            string parentInfoStr = m_FSLClient.ExecuteQuery(parentInfoQuery, Measures.PARENT_QUERY, measurments);
            
            deselializeQueryResult(parentUtils, parentInfoStr);

            watch.Stop();
            
            measurments.addMeasure(Measures.PARENT_PROCESSING, watch.ElapsedMilliseconds);
            watch.Reset();
        }

        private void getSTMs()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            sObjectUtils STMServiceUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(ServiceTerritoryMember), m_ABData);
            string stmQuery = STMServiceUtils.getQuery(m_Request);
            string stmStr = m_FSLClient.ExecuteQuery(stmQuery, Measures.STM_QUERY, measurments);
            
            deselializeQueryResult(STMServiceUtils, stmStr);
            
            string stmAdditionalMembersQuery = STMServiceUtils.getQuery(m_Request);
            string stmAdditionalMembersQueryStr = m_FSLClient.ExecuteQuery(stmAdditionalMembersQuery, Measures.STM_QUERY, measurments);
            
            deselializeQueryResult(STMServiceUtils, stmAdditionalMembersQueryStr);
            watch.Stop();
            
            measurments.addMeasure(Measures.STM_PROCESSING, watch.ElapsedMilliseconds);
            watch.Reset();
        }

        private void getMSTServices()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            if (m_ABData.TimeDependeciesByRootId == null || !m_ABData.TimeDependeciesByRootId.Any())
            {
                watch.Stop();
                
                measurments.addMeasure(Measures.MST_PROCESSING, watch.ElapsedMilliseconds);
                watch.Reset();
                return;
            }

            sObjectUtils MSTServiceUtils = ObjectUtilsFactory.CreateUtilsByType(null, m_ABData,"MSTServiceUtils");
            string serviceDependencyQuery = MSTServiceUtils.getQuery(m_Request);
            string serviceDependencyStr = m_FSLClient.ExecuteQuery(serviceDependencyQuery, Measures.MST_QUERY, measurments);
            
            deselializeQueryResult(MSTServiceUtils, serviceDependencyStr);
            
            watch.Stop();
            
            measurments.addMeasure(Measures.MST_PROCESSING, watch.ElapsedMilliseconds);
            watch.Reset();
        }

        private void getTimeDependencies()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            sObjectUtils timeDependenciesUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(Time_Dependency__c));
            string serviceDependencyQuery = timeDependenciesUtils.getQuery(m_Request);
            if (string.IsNullOrEmpty(serviceDependencyQuery))
                return;
            
            string serviceDependencyStr = m_FSLClient.ExecuteQuery(serviceDependencyQuery, Measures.DEPENDENCIES_QUERY, measurments);

            deselializeQueryResult(timeDependenciesUtils, serviceDependencyStr);
            
            string rootDependenciesQuery = timeDependenciesUtils.getQuery(m_Request);
            if (string.IsNullOrEmpty(rootDependenciesQuery))
            {
                watch.Stop();
               
                measurments.addMeasure(Measures.DEPENDENCIES_PROCESSING, watch.ElapsedMilliseconds);
                watch.Reset();
                return;
            }

            string rootDependenciesStr = m_FSLClient.ExecuteQuery(rootDependenciesQuery, Measures.DEPENDENCIES_QUERY, measurments);

            deselializeQueryResult(timeDependenciesUtils, rootDependenciesStr);
            watch.Stop();
            
            measurments.addMeasure(Measures.DEPENDENCIES_PROCESSING, watch.ElapsedMilliseconds);
            watch.Reset();
        }

        private void getService()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            sObjectUtils serviceUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(ServiceAppointment), m_ABData);
            string serviceQuery = serviceUtils.getQuery(m_Request);

            string serviceStr = m_FSLClient.ExecuteQuery(serviceQuery, Measures.SA_QUERY, measurments);

            deselializeQueryResult(serviceUtils, serviceStr);
            m_ABData.ServiceToSchedule = m_ABData.ServicesById[m_Request.ServiceID];
            
            watch.Stop();
            
            measurments.addMeasure(Measures.SA_PROCESSING, watch.ElapsedMilliseconds);
            watch.Reset();
        }

        private void getPolicyRulesAndObjectives()
        {
            //getPolicyRulesAndObjectivesSync();
            getPolicyRulesAndObjectivesASync();
        }
        
        private void getPolicyRulesAndObjectivesSync()
        {
            getRulesByPolicy(false);
            getObjectivesByPolicy(false);
        }
        private async void getPolicyRulesAndObjectivesASync()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            List<Task> tasks = new List<Task>()
            {
                Task.Run(() => getRulesByPolicy(true)),
                Task.Run(() => getObjectivesByPolicy(true))
            };
            

            Task.WaitAll(tasks.ToArray());

            await Task.CompletedTask;
            
            watch.Stop();
            
            measurments.addMeasure(Measures.OBJECTIVES_RULES_PARALLEL, watch.ElapsedMilliseconds);
            watch.Reset();
        }
        
        private Task getObjectivesByPolicy(bool async = false)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            sObjectUtils objectivesUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(Scheduling_Policy_Goal__c));
            string objectivesQuery = objectivesUtils.getQuery(m_Request);
            
            string objectivesStr = m_FSLClient.ExecuteQuery(objectivesQuery, Measures.OBJECTIVES_QUERY, measurments, false, async);
            
            deselializeQueryResult(objectivesUtils, objectivesStr);
            watch.Stop();

            if(!async)
                measurments.addMeasure(Measures.OBJECTIVES_PROCESSING, watch.ElapsedMilliseconds);
            
            watch.Reset();
            
            return Task.CompletedTask;
        }

        private Task getRulesByPolicy(bool async = false)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            sObjectUtils rulesUtils = ObjectUtilsFactory.CreateUtilsByType(typeof(Work_Rule__c));
            getWorkRulesObjectsByPolicy(rulesUtils, async);
            getGapRuleRelatedRules();
            
            watch.Stop();

            if(!async)
                measurments.addMeasure(Measures.RULES_PROCESSING, watch.ElapsedMilliseconds);
            
            watch.Reset();

            return Task.CompletedTask;
        }

        private void getWorkRulesObjectsByPolicy(sObjectUtils i_ObjectUtils, bool async)
        {
            string rulesQuery = i_ObjectUtils.getQuery(m_Request);
            string rulesStr = m_FSLClient.ExecuteQuery(rulesQuery, Measures.RULES_QUERY, measurments, false, async);

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

        private void deselializeQueryResult(sObjectUtils i_ObjectUtils, string i_QueryResultStr, AdditionalObjectsUtils.eAdditionalObjectQuery i_ObjectToDeserialize = default, bool async = false)
        {
            i_ObjectUtils.Deserialize(i_QueryResultStr, m_ABData, i_ObjectToDeserialize, async); 
        }

        private void getGapRuleRelatedRules()
        {
            //TODO: missing a rule query
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
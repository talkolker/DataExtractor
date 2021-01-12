using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Runtime.SharedInterfaces;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;
using Processor;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.Requests;

namespace SalesforceLibrary.DataModel.Utils.sObjectUtils
{
    public class STMUtils : sObjectUtils
    {
        public const double MILE_TO_KM_RATIO = 1.609344;
        private string m_AdditionalMembersQuery;
        private bool doAdditionalMembersQuery = false;
        private AppointmentBookingData m_ABData;
        private List<ServiceTerritoryMember> m_TerritoryMembers;
        private AppointmentBookingRequest m_Request;
        private DateTime m_DateTimeNow;
        private DateTime m_Start;
        private DateTime m_Finish;

        enum eTravelUnit
        {
            Miles,
            Kilometers
        }
        
        private class DeserializedQueryResult
        {
            public Double? totalSize;
            public Boolean? done;
            public List<ServiceTerritoryMember> records;
        }

        public STMUtils(AppointmentBookingData i_ABData)
        {
            m_ABData = i_ABData;
            m_DateTimeNow = DateTime.Now;
        }
        
        public override void Deserialize(string i_QueryResult, AppointmentBookingData i_ABData,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default, bool async = false)
        {
            DeserializedQueryResult deserializedQuery =
                JsonConvert.DeserializeObject<DeserializedQueryResult>(i_QueryResult);

            foreach (ServiceTerritoryMember stm in deserializedQuery.records)
            {
                if (stm.m_JSONAdditionalData.TryGetValue("ServiceResource", out var resourceToken))
                {
                    stm.ServiceResource = resourceToken.ToObject<ServiceResource>();
                }
            }

            if(m_TerritoryMembers == null)
                m_TerritoryMembers = new List<ServiceTerritoryMember>();
            
            m_TerritoryMembers.AddRange(deserializedQuery.records);
            i_ABData.TerritoryMembers = m_TerritoryMembers;
            i_ABData.STMResourcesIDs = m_ABData.STMResourcesIDs;
        }

        public override string getQuery(AppointmentBookingRequest i_Request = null,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            m_Request = i_Request;
            
            if (doAdditionalMembersQuery)
            {
                return getAdditionalMembersQuery();
            }
            
            string queryStr = "SELECT ServiceResource.Id, ServiceResource.ResourceType, ServiceResource.ServiceCrewId, ServiceResource.Travel_Speed__c"+
                               ",ServiceResource.IsCapacityBased,ServiceResource.IsActive,ServiceResource.Name," +
                               "ServiceTerritory.Address,ServiceTerritory.Internal_SLR_Geolocation__c,ServiceTerritory.Longitude" +
                               ",ServiceTerritory.Latitude,Id,ServiceResourceId,ServiceTerritory.OperatingHours.TimeZone" +
                               ",ServiceTerritory.Internal_SLR_Geolocation__Latitude__s,ServiceTerritory.Internal_SLR_Geolocation__Longitude__s" +
                               ",OperatingHoursId,OperatingHours.TimeZone,EffectiveEndDate,EffectiveStartDate,Latitude,Longitude," +
                               "Internal_SLR_HomeAddress_Geolocation__Latitude__s,Internal_SLR_HomeAddress_Geolocation__Longitude__s" +
                               ",Internal_SLR_HomeAddress_Geolocation__c,TerritoryType,ServiceTerritoryId,Address";

            m_AdditionalMembersQuery = "Select ServiceResource.Id, ServiceResource.IsActive,ServiceResource.Name,ServiceTerritory.Internal_SLR_Geolocation__c,"+
                                            "ServiceTerritory.Longitude,ServiceTerritory.Latitude,Id,ServiceResourceId," +
                                            "ServiceTerritory.OperatingHours.TimeZone,ServiceTerritory.Internal_SLR_Geolocation__Latitude__s," +
                                            "ServiceTerritory.Internal_SLR_Geolocation__Longitude__s,OperatingHoursId,OperatingHours.TimeZone," +
                                            "EffectiveEndDate,EffectiveStartDate,Latitude,Longitude,Internal_SLR_HomeAddress_Geolocation__Latitude__s," +
                                            "Internal_SLR_HomeAddress_Geolocation__Longitude__s,Internal_SLR_HomeAddress_Geolocation__c," +
                                            "TerritoryType,ServiceTerritoryId ";

            HashSet<String> relevanceFields = new HashSet<String>();
            ServiceAppointment service = m_ABData.ServiceToSchedule;
            string notPartOfRelevenceGroupStr = "(";
            bool atLeastOneLocationRuleIsActive = false;
            bool travelRuleIsActive = false;
            double? travelDistance = null;
            double? latestUpdatedTravelDistance;
            bool addedOneOfTheRulesToTheQuery = false;
            bool serviceHasGeo = service.Latitude != null && service.Longitude != null;
            List<Work_Rule__c> enhancedRules = new List<Work_Rule__c>();
            List<Work_Rule__c> rulesListFileteredBySARelevance = new List<Work_Rule__c>();
            string territoryId = m_ABData.ServiceToSchedule.ServiceTerritory.Id;

            rulesListFileteredBySARelevance = initiateRulesWithRelvanceGroupList(m_ABData.RulesByDevName, service);

            foreach (Work_Rule__c rule in rulesListFileteredBySARelevance)
            {
                string resGroupFieldValue = rule.Resource_Group_Field__c;
                if( rule.DeveloperName.Equals("Match_Location_Service") || rule.DeveloperName.Equals("Field_Working_Locations"))
                    atLeastOneLocationRuleIsActive = true;

                if(serviceHasGeo && rule.DeveloperName.Equals("Max_Travel_From_Home_To_Service") && rule.Maximum_Travel_From_Home_Type__c.Equals("Distance") && rule.Maximum_Travel_From_Home__c != null)
                {
                    travelRuleIsActive = true;
                    latestUpdatedTravelDistance = (i_Request.TravelUnit == eTravelUnit.Miles.ToString()) ? (rule.Maximum_Travel_From_Home__c * MILE_TO_KM_RATIO) : rule.Maximum_Travel_From_Home__c;
                    if (travelDistance == null || travelDistance < latestUpdatedTravelDistance){
                        travelDistance = latestUpdatedTravelDistance;
                        travelDistance++;
                    }
                }
                
                if (!string.IsNullOrEmpty(resGroupFieldValue))
                {
                    relevanceFields.Add(resGroupFieldValue.ToLower());

                    if (rule.DeveloperName.Equals("Match_Location_Service") || rule.DeveloperName.Equals("Field_Working_Locations") || (rule.DeveloperName.Equals("Max_Travel_From_Home_To_Service") && rule.Maximum_Travel_From_Home_Type__c.Equals("Distance")))
                    {
                        if(notPartOfRelevenceGroupStr != "(" ) {
                            notPartOfRelevenceGroupStr += " AND ";
                        }
                        notPartOfRelevenceGroupStr += resGroupFieldValue.ToLower() + " = false";
                    }
                }
                
                if(rule.DeveloperName.Equals("Enhanced_Match_Service")){
                    enhancedRules.Add(rule);
                }
            }
            
            if(notPartOfRelevenceGroupStr != "(")
                notPartOfRelevenceGroupStr += $" AND TerritoryType != \'S\' AND ServiceTerritoryId = {territoryId} )";
            else
                notPartOfRelevenceGroupStr += ")";

            foreach(KeyValuePair<string, List<Service_Goal__c>> objByDevName in m_ABData.ObjectivesByDevName)
            {
                foreach (Service_Goal__c obj in objByDevName.Value)
                {
                    String objGroupFieldValue = obj.Resource_Group_Field__c;

                    if (!string.IsNullOrEmpty(objGroupFieldValue))
                        relevanceFields.Add(objGroupFieldValue.ToLower());
                }
            }
            
            if(relevanceFields.Any())
            {
                foreach(String field in relevanceFields)
                {
                    queryStr += "," + field;
                    m_AdditionalMembersQuery += "," + field;
                }
            }
            
            calculateHorizonByMaxDaysSearchSlot(service, m_DateTimeNow, i_Request.SearchSlotsMaxDays, out m_Start, out m_Finish);

            notPartOfRelevenceGroupStr = notPartOfRelevenceGroupStr != "()" ? " OR " + notPartOfRelevenceGroupStr : string.Empty;

            queryStr += " FROM ServiceTerritoryMember WHERE ";
            
            if( atLeastOneLocationRuleIsActive && territoryId != null ) {
                addedOneOfTheRulesToTheQuery = true;

                if(travelRuleIsActive) {
                    queryStr += $"(( ServiceTerritoryId = '{territoryId}' And ";    
                } else {
                    queryStr += $"( ServiceTerritoryId = '{territoryId}' ";    
                }
            }
            
            if(travelRuleIsActive) {
                if(!addedOneOfTheRulesToTheQuery) {
                    queryStr += "(";
                }

                queryStr += $" (TerritoryType = \'S\' Or ( Latitude <> null And Latitude <> 0 And Longitude <> null And Longitude <> 0 And (DISTANCE(Address, GEOLOCATION(' + {service.Latitude} + ',' + {service.Longitude} + '),\'km\') < ' + travelDistance + ') ) OR ";
                queryStr += $" ( ServiceTerritory.Latitude <> null And ServiceTerritory.Latitude <> 0 And ServiceTerritory.Longitude <> null And ServiceTerritory.Longitude <> 0 And (DISTANCE(ServiceTerritory.Address, GEOLOCATION(' + {service.Latitude} + ',' + {service.Longitude} + '),\'km\') < ' + travelDistance + ') ))";

                if(addedOneOfTheRulesToTheQuery)
                    queryStr += ')';

                addedOneOfTheRulesToTheQuery = true;
            }

            if(addedOneOfTheRulesToTheQuery) {
                queryStr += notPartOfRelevenceGroupStr + ") AND ";
            }

            queryStr += $"( NOT ((EffectiveStartDate <> NULL AND EffectiveStartDate > {formatDate(m_Finish)}) OR "+
                        $"(EffectiveEndDate <> NULL AND EffectiveEndDate < {formatDate(m_Start)})) ) And ServiceResource.IsActive = true";

            //TODO: implement for Enhanced Match Rule
            /*
            if(enhancedRules != null && enhancedRules.Count > 0)
            {
                List<string> queriesToFilterResources = getQueriesToFilterResourcesAcoordingToEnhancedRules(service, enhancedRules); 
                foreach(string queryToFilterResource in queriesToFilterResources){
                    queryStr += queryToFilterResource;
                }
            }
            */

            doAdditionalMembersQuery = true;
            queryStr = formatQueryString(queryStr);
            return queryStr;
        }

        private List<Work_Rule__c> initiateRulesWithRelvanceGroupList(
            Dictionary<string, List<Work_Rule__c>> i_RulesByDevName, ServiceAppointment i_Service)
        {
            List<Work_Rule__c> rulesFilteredByRelevanceGroup = new List<Work_Rule__c>();
            foreach (List<Work_Rule__c> rules in i_RulesByDevName.Values)
            {
                foreach (Work_Rule__c rule in rules.Where(rule => rule.Object_Group_Field__c == null || ServiceAppointmentUtils.getBooleanField(i_Service, rule.Object_Group_Field__c)))
                {
                    rulesFilteredByRelevanceGroup.Add(rule);
                }
            }

            return rulesFilteredByRelevanceGroup;
        }

        private string getAdditionalMembersQuery()
        {
            HashSet<string> membersNotToGet = new HashSet<string>();
            HashSet<string> resourcesIds = new HashSet<string>();
            
            foreach(ServiceTerritoryMember member in m_TerritoryMembers) {
                resourcesIds.Add(member.ServiceResource.Id);
                membersNotToGet.Add(member.Id);
            }

            string resourcesIdsStr = formatIdList(resourcesIds.ToList());
            string membersNotToGetStr = formatIdList(membersNotToGet.ToList());

            m_ABData.STMResourcesIDs = resourcesIds;

            m_AdditionalMembersQuery += $" From ServiceTerritoryMember Where Id not in ({membersNotToGetStr}) and "+
                                        $"ServiceResourceId in ({resourcesIdsStr}) And EffectiveStartDate <= {formatDate(m_Finish)} " +
                                        $"And (EffectiveEndDate = null Or EffectiveEndDate >= {formatDate(m_Start)}) " +
                                        "and (TerritoryType = \'P\' or TerritoryType = \'R\')";
            //List<ServiceTerritoryMember> additionalMembers = Database.query(additinalMembersQuery);
//
            //territoryMembers.addAll(additionalMembers);
        //
            //return GetNonSecondaryObjects(territoryMembers);
            
            m_AdditionalMembersQuery = formatQueryString(m_AdditionalMembersQuery);
            return m_AdditionalMembersQuery;
        }
    }
}
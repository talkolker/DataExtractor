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
    public class MSTServiceUtils : sObjectUtils
    {
        private HashSet<string> m_MSTServiceIds;
        
        private class DeserializedQueryResult
        {
            public Double? totalSize;
            public Boolean? done;
            public List<ServiceAppointment> records;
        }

        public MSTServiceUtils(AppointmentBookingData i_ABData)
        {
            m_MSTServiceIds = new HashSet<string>();

            foreach (List<Time_Dependency__c> dependenciesByRoot in i_ABData.TimeDependeciesByRootId.Values)
            {
                foreach (Time_Dependency__c dependency in dependenciesByRoot)
                {
                    m_MSTServiceIds.Add(dependency.Service_Appointment_1__c);
                    m_MSTServiceIds.Add(dependency.Service_Appointment_2__c);
                }
            }
        }
        
        public override void Deserialize(string i_QueryResult, AppointmentBookingData i_ABData,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            DeserializedQueryResult deserializedQuery =
                JsonConvert.DeserializeObject<DeserializedQueryResult>(i_QueryResult);
            
            ServiceAppointmentUtils serviceUtils = new ServiceAppointmentUtils(i_ABData);
            Dictionary<string, ServiceAppointment> deserializedServices = serviceUtils.parseAdditionalData(deserializedQuery.records);

            foreach (KeyValuePair<string, List<Time_Dependency__c>> dependencyTreeByRoot in i_ABData.TimeDependeciesByRootId)
            {
                foreach (Time_Dependency__c dependency in dependencyTreeByRoot.Value)
                {
                    if (!i_ABData.ServicesById.ContainsKey(dependency.Service_Appointment_1__c))
                        i_ABData.ServicesById[dependency.Service_Appointment_1__c] = deserializedServices[dependency.Service_Appointment_1__c];

                    dependency.Service_Appointment_1__r = deserializedServices[dependency.Service_Appointment_1__c];
                    
                    if (!i_ABData.ServicesById.ContainsKey(dependency.Service_Appointment_2__c))
                        i_ABData.ServicesById[dependency.Service_Appointment_2__c] = deserializedServices[dependency.Service_Appointment_2__c];

                    dependency.Service_Appointment_2__r = deserializedServices[dependency.Service_Appointment_2__c];
                    
                    if(dependency.Service_Appointment_1__c == dependency.Root_Service_Appointment__c)
                        dependency.Root_Service_Appointment__r = deserializedServices[dependency.Service_Appointment_1__c];
                    
                    if(dependency.Service_Appointment_2__c == dependency.Root_Service_Appointment__c)
                        dependency.Root_Service_Appointment__r = deserializedServices[dependency.Service_Appointment_2__c];
                }
            }
        }

        public override string getQuery(AppointmentBookingRequest i_Request = null,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            string serviceIdStr = formatIdList(m_MSTServiceIds.ToList());
            string query = "SELECT Id,Related_Service__c,Same_Day__c,Same_Resource__c,Time_Dependency__c,"+
                           "AppointmentNumber,SchedStartTime, SchedEndTime,ServiceTerritory.OperatingHours.TimeZone, " +
                           "ServiceTerritoryId, Duration, ParentRecordId, (SELECT AssignedResourceNumber" +
                           ",ServiceResourceId,EstimatedTravelTimeFrom__c,EstimatedTravelTime,EstimatedTravelDistanceFrom__c," +
                           " EstimatedTravelDistanceTo__c FROM ServiceResources ORDER BY CreatedDate ASC) " +
                           $"FROM ServiceAppointment WHERE Id in ({serviceIdStr})";

            return formatQueryString(query);
        }
    }
}
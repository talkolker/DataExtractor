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
    public class AdditionalObjectsUtils : sObjectUtils
    {
        private AppointmentBookingData m_ABData;
        private string m_Resources;
        private DateTime m_Start;
        private DateTime m_Finish;
        private HashSet<string> m_CalendarIDs;

        private class DeserializedQueryResult
        {
            public Double? totalSize;
            public Boolean? done;
            public List<sObject> records;
        }

        public enum eAdditionalObjectQuery
        {
            ServicesInResourcesTimeDomain,
            ResourcesAdditionalObjects,
            ResourceTerritories,
            Capacities,
            Calendars
        }
        
        public AdditionalObjectsUtils(AppointmentBookingData i_ABData)
        {
            m_ABData = i_ABData;
            m_Resources = formatIdList(m_ABData.CandidatesById.Keys.ToList());
        }
        public override void Deserialize(string i_QueryResult, AppointmentBookingData i_ABData,
            eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            DeserializedQueryResult deserializedQuery =
                JsonConvert.DeserializeObject<DeserializedQueryResult>(i_QueryResult);
            
            switch (i_AdditionalObjQuery)
            {
                case eAdditionalObjectQuery.ServicesInResourcesTimeDomain:
                    i_ABData.ABAdditionalObjects.ServicesById =
                    deserializedQuery.records.ToDictionary(record => record.Id);
                    break;
                
                case eAdditionalObjectQuery.ResourcesAdditionalObjects:
                    i_ABData.ABAdditionalObjects.ResBreaksAndShiftsByResId =
                        deserializedQuery.records.ToDictionary(record => record.Id);
                    break;
                
                case eAdditionalObjectQuery.ResourceTerritories:
                    deserializedResourceTerritories(i_ABData, deserializedQuery.records);
                    break;
                
                case eAdditionalObjectQuery.Capacities:
                    i_ABData.ABAdditionalObjects.Capacities =
                        deserializedQuery.records;
                    break;
                
                case eAdditionalObjectQuery.Calendars:
                    i_ABData.ABAdditionalObjects.Calendars =
                        deserializedQuery.records;
                    break;
                
                default:
                    break;
            }
        }

        private void deserializedResourceTerritories(AppointmentBookingData i_ABData, List<sObject> i_DeserializedRecords)
        {
            m_CalendarIDs = new HashSet<string>();
            foreach (sObject stm in i_DeserializedRecords)
            {
                ServiceTerritory territory;
                string terrOpHours = "";
                string stmOpHours = "";
                if (stm.m_JSONAdditionalData.TryGetValue("OperatingHoursId", out var opHoursToken))
                {
                    stmOpHours = opHoursToken.ToObject<string>();
                }
                
                if (stm.m_JSONAdditionalData.TryGetValue("ServiceTerritory", out var resourceToken))
                {
                    territory = resourceToken.ToObject<ServiceTerritory>();
                    terrOpHours = territory.OperatingHoursId;
                }

                m_CalendarIDs.Add(!string.IsNullOrEmpty(stmOpHours) ? stmOpHours : terrOpHours);
            }
        }

        public override string getQuery(AppointmentBookingRequest i_Request = null,
            eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            ServiceAppointment service = m_ABData.ServiceToSchedule;
            m_Start = service.EarliestStartTime.Value;
            m_Finish = service.DueDate.Value;
            calculateHorizonByMaxDaysSearchSlot(service, DateTime.Now, i_Request.SearchSlotsMaxDays, out m_Start,
                out m_Finish);
            
            string query;

            switch (i_AdditionalObjQuery)
            {
                case eAdditionalObjectQuery.ServicesInResourcesTimeDomain:
                    query = getServicesInResourcesTimeDomainQuery();
                    break;
                
                case eAdditionalObjectQuery.ResourcesAdditionalObjects:
                    query = getResourcesQueryForAdditionalObjects(i_Request);
                    break;
                
                case eAdditionalObjectQuery.ResourceTerritories:
                    query = getResourceTerritoriesQuery(i_Request);
                    break;
                
                case eAdditionalObjectQuery.Capacities:
                    query = getCapacitiesQuery(i_Request);
                    break;
                
                case eAdditionalObjectQuery.Calendars:
                    query = getCalendarsQuery(i_Request);
                    break;
                
                default:
                    query = String.Empty;
                    break;
            }
            
            return formatQueryString(query);
        }

        private string getCalendarsQuery(AppointmentBookingRequest iRequest)
        {
            string calendarIds = formatIdList(m_CalendarIDs.ToList());
            string query = "Select Id, TimeZone, (Select Designated_Work_Boolean_Fields__c,StartTime, EndTime, "+
                           "DayOfWeek, Type From TimeSlots order by DayOfWeek, StartTime) " +
                           $"From OperatingHours where id in ({calendarIds})";

            return query;
        }

        private string getCapacitiesQuery(AppointmentBookingRequest iRequest)
        {
            string start = m_Start.Date.ToString("yyyy-MM-dd");
            string finish = m_Finish.Date.ToString("yyyy-MM-dd");
            string query = "Select CapacityInHours,CapacityInWorkItems,TimePeriod,EndDate,MinutesUsed__c,HoursInUse__c"+
                           ",StartDate,ServiceResourceId,Work_Items_Allocated__c From ServiceResourceCapacity " +
                           $"where ((StartDate >= {start} " +
                           $"AND StartDate <= {finish} AND TimePeriod = 'Day')" +
                           $" or (StartDate >= {start}" +
                           $" AND StartDate <= {finish} AND TimePeriod = 'Month') " +
                           $"or (StartDate >= {start}" +
                           $" AND StartDate <= {finish} AND TimePeriod = 'Week'))" +
                           $" and (ServiceResourceId in ({m_Resources}))";

            return query;
        }

        private string getResourceTerritoriesQuery(AppointmentBookingRequest i_Request)
        {
            DateTime start = m_Start.AddDays(-1).Date;
            DateTime finish = m_Finish.AddDays(2).Date;
            string query = "Select Internal_SLR_HomeAddress_Geolocation__Latitude__s,"+
                           "Internal_SLR_HomeAddress_Geolocation__Longitude__s," +
                           "ServiceTerritory.Internal_SLR_Geolocation__Latitude__s," +
                           "ServiceTerritory.Internal_SLR_Geolocation__Longitude__s," +
                           "ServiceTerritory.Longitude,ServiceTerritory.Latitude, Latitude, Longitude," +
                           "OperatingHoursId,TerritoryType, ServiceResourceId, ServiceTerritory.OperatingHoursId," +
                           " EffectiveEndDate, EffectiveStartDate, ServiceTerritory.OperatingHours.TimeZone " +
                           $"From ServiceTerritoryMember where ServiceResourceId in ({m_Resources}) and " +
                           $"((EffectiveStartDate >= {formatDate(start)} and EffectiveStartDate <= {formatDate(finish)}) or " +
                           $"(EffectiveEndDate >= {formatDate(start)} and EffectiveEndDate <= {formatDate(finish)}) or " +
                           $"((EffectiveStartDate < {formatDate(start)} or EffectiveStartDate = null) and" +
                           $" (EffectiveEndDate > {formatDate(finish)} or EffectiveEndDate = null)))";

            return query;
        }

        private string getServicesInResourcesTimeDomainQuery()
        {
            //TODO: add additional count rule fields
            //TODO: add relevant part of query for capacity count rule fields
            string servicesToSkip = "''";
            string query = "Select Schedule_over_lower_priority_appointment__c,Id,Status,Duration, DurationType, "+
                           "Pinned__c, ParentRecordId ,AppointmentNumber, SchedStartTime, ServiceTerritory.OperatingHours.TimeZone," +
                           " (Select Estimated_Travel_Time_To_Source__c," +
                           "Estimated_Travel_Time_From_Source__c,ServiceResourceId,EstimatedTravelTimeFrom__c," +
                           "EstimatedTravelTime, EstimatedTravelDistanceFrom__c,EstimatedTravelDistanceTo__c," +
                           "ServiceResource.ResourceType From ServiceResources ORDER BY ServiceResource.ResourceType DESC NULLS LAST, " +
                           "CreatedDate ASC NULLS LAST), SchedEndTime, Latitude, Longitude, InternalSLRGeolocation__Latitude__s, " +
                           "InternalSLRGeolocation__Longitude__s ,  (Select Service_Appointment_1__c, Service_Appointment_2__c " +
                           "From Time_Dependencies__r Where Dependency__c= 'Immediately Follow' ), " +
                           "(Select Service_Appointment_1__c, Service_Appointment_2__c From Time_Dependencies_2__r " +
                           "Where Dependency__c= 'Immediately Follow' ) From ServiceAppointment Where" +
                           $" ((SchedStartTime >= {formatDate(m_Start)} and SchedStartTime <= {formatDate(m_Finish)}) or " +
                           $"(SchedEndTime >= {formatDate(m_Start)} and SchedEndTime <= {formatDate(m_Finish)}) or " +
                           $"(SchedStartTime < {formatDate(m_Start)} and SchedEndTime > {formatDate(m_Finish)})) " +
                           $"And Id in (Select ServiceAppointmentId From AssignedResource Where ServiceResourceId in ({m_Resources})) " +
                           $"and Id not in ({servicesToSkip}) order by SchedStartTime, SchedEndTime";

            return query;
        }

        private string getResourcesQueryForAdditionalObjects(AppointmentBookingRequest i_Request)
        {
            DateTime start = m_Start.AddDays(-1).Date;
            DateTime finish = m_Finish.AddDays(2).Date;
            string resourceIds = formatIdList(m_ABData.CandidatesById.Keys.ToList());
            
            String innerAbsencesQuery = "(select Id, Estimated_Travel_Time_To_Source__c,Estimated_Travel_Time_From_Source__c,"+
                                        "Type, RecordType.DeveloperName, AbsenceNumber, Start, EstTravelTimeFrom__c, " +
                                        "EstTravelTime__c, EstimatedTravelDistanceFrom__c, EstimatedTravelDistanceTo__c, " +
                                        "ResourceId, End,Latitude, Longitude, InternalSLRGeolocation__Latitude__s, " +
                                        "InternalSLRGeolocation__Longitude__s From ResourceAbsences";
            innerAbsencesQuery += $" where ((Start >= {formatDate(start)} and Start <= {formatDate(finish)})or(End >= {formatDate(start)} and End <= {formatDate(finish)}) "+
                                  $"or (Start < {formatDate(start)} and End > {formatDate(finish)})) and RecordType.DeveloperName in ( \'Non_Availability\' ) ";
            if(i_Request.ApprovedAbsences){
                innerAbsencesQuery += " and ( ( Approved__c = true and RecordType.DeveloperName = \'Non_Availability\')OR RecordType.DeveloperName = \'Break\')";
            }
            innerAbsencesQuery += " order by Start, End)";

            String innerShiftsQuery = "(SELECT Id, StartTime, EndTime, ServiceResourceId, ServiceTerritoryId, StatusCategory FROM ShiftServiceResources WHERE ("+
                                      $"(StartTime >= {formatDate(start)} AND StartTime <= {formatDate(finish)}) "+
                                      " OR "+
                                      $"(EndTime >= {formatDate(start)} AND EndTime <= {formatDate(finish)}) "+
                                      " OR "+
                                      $"(StartTime <= {formatDate(start)} AND EndTime >  {formatDate(finish)}) "+
                                      ")"+
                                      "Order by StartTime)";

            String externalQuery = "SELECT Id, " + innerAbsencesQuery + "," + innerShiftsQuery + 
                                   $" FROM ServiceResource WHERE Id in ({resourceIds})";

            return externalQuery;
        }
    }
}
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Processor;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.Requests;

namespace SalesforceLibrary.DataModel.Utils.sObjectUtils
{
    public class OperatingHoursUtils : sObjectUtils
    {
        private AppointmentBookingData m_ABData;

        private class DeserializedQueryResult
        {
            public Double? totalSize;
            public Boolean? done;
            public List<OperatingHours> records;
        }
        public OperatingHoursUtils(AppointmentBookingData i_ABData)
        {
            m_ABData = i_ABData;
        }
        public override void Deserialize(string i_QueryResult, AppointmentBookingData i_ABData,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default, bool async = false)
        {
            DeserializedQueryResult deserializedQuery =
                JsonConvert.DeserializeObject<DeserializedQueryResult>(i_QueryResult);

            i_ABData.VisitingHours = deserializedQuery.records;
        }

        public override string getQuery(AppointmentBookingRequest i_Request = null,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            ServiceParent parent = null;
            foreach (ServiceParent parentObj in m_ABData.ServiceParent.Values)
            {
                if (parentObj.Id == m_ABData.ServiceToSchedule.ParentRecordId)
                {
                    parent = parentObj;
                    break;
                }
            }

            if (parent == null)
            {
                throw new Exception("Service parent was not retrieved properly");
            }
            
            //TODO: make generic for all types of parent
            string visitingHoursCalId = parent.VisitingHours__c;
            if(visitingHoursCalId == null)
                return String.Empty;
            
            string query = "Select id,TimeZone, "+
                           "(Select DayOfWeek, Type, StartTime, EndTime From TimeSlots order by DayOfWeek, StartTime) " +
                           $"From OperatingHours where id = '{visitingHoursCalId}' limit 1]";

            return formatQueryString(query);
        }
    }
}
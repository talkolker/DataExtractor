using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Processor;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.Requests;

namespace SalesforceLibrary.DataModel.Utils.sObjectUtils
{
    public class LicensedUsersUtils : sObjectUtils
    {
        private AppointmentBookingData m_ABData;
        private HashSet<string> m_RelatedRecordIds;
        
        private class DeserializedQueryResult
        {
            public Double? totalSize;
            public Boolean? done;
            public List<PermissionSetAssignment> records;
        }

        public LicensedUsersUtils(AppointmentBookingData i_ABData)
        {
            m_ABData = i_ABData;
        }
        
        public override void Deserialize(string i_QueryResult, AppointmentBookingData i_ABData,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            DeserializedQueryResult deserializedQuery =
                JsonConvert.DeserializeObject<DeserializedQueryResult>(i_QueryResult);

            foreach (PermissionSetAssignment record in deserializedQuery.records)
            {
                m_RelatedRecordIds.Remove(record.AssigneeId);
            }

            i_ABData.UnLicensedUsers = m_RelatedRecordIds;
        }

        public override string getQuery(AppointmentBookingRequest i_Request = null,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            m_RelatedRecordIds = new HashSet<string>();
            foreach (string resourceId in m_ABData.STMResourcesIDs)
            {
                m_RelatedRecordIds.Add(m_ABData.CandidatesById[resourceId].UserId);
            }
            
            
            string query = $"Select AssigneeId From PermissionSetAssignment Where AssigneeId in ({formatIdList(m_RelatedRecordIds.ToList())}) "+
                           "and PermissionSet.IsOwnedByProfile = false and" +
                           " PermissionSet.PermissionsFieldServiceScheduling = true";

            return formatQueryString(query);
        }
    }
}
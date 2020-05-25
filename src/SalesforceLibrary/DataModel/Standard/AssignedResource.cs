using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using System;
using Newtonsoft.Json.Linq;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class AssignedResource : sObject
    {
        private ServiceAppointment m_ServiceAppointment;
        private ServiceCrew m_ServiceCrew;
        private ServiceResource m_ServiceResource;

        [JsonIgnore]
        public TimeSpan? ActualTravelTime { get; set; }

        [JsonProperty("EstimatedTravelTime")]
        private double? estimatedTravelTimeInMinutes
        {
            get { return NumericFields["EstimatedTravelTime"]; }
            set { NumericFields["EstimatedTravelTime"] = value; }
        }

        [JsonIgnore]
        public TimeSpan? EstimatedTravelTime
        {
            get
            {
                TimeSpan? calculatedValue = null;
                if (estimatedTravelTimeInMinutes.HasValue)
                {
                    calculatedValue = TimeSpan.FromMinutes(estimatedTravelTimeInMinutes.Value);
                }
                return calculatedValue;
            }
            set
            {
                if (value == null)
                    estimatedTravelTimeInMinutes = null;
                else
                    estimatedTravelTimeInMinutes = Math.Ceiling(value.Value.TotalMinutes);
            }
        }

        [JsonProperty]
        private string ServiceAppointmentId { get; set; }

        [JsonIgnore]
        public ServiceAppointment ServiceAppointment
        {
            get
            {
                return m_ServiceAppointment;
            }
            set
            {
                m_ServiceAppointment = value;
                ServiceAppointmentId = m_ServiceAppointment?.Id;
            }
        }

        [JsonProperty]
        private string ServiceCrewId { get; set; }

        [JsonIgnore]
        public ServiceCrew ServiceCrew
        {
            get
            {
                return m_ServiceCrew;
            }
            set
            {
                m_ServiceCrew = value;
                ServiceCrewId = m_ServiceCrew?.Id;
            }
        }

        [JsonProperty]
        private string ServiceResourceId { get; set; }

        [JsonIgnore]
        public ServiceResource ServiceResource
        {
            get
            {
                return m_ServiceResource;
            }
            set
            {
                m_ServiceResource = value;
                ServiceResourceId = m_ServiceResource?.Id;
            }
        }

        public AssignedResource() :
            this(null)
        { }

        public AssignedResource(string i_ObjectId) :
            base(i_ObjectId)
        {
            m_ServiceAppointment = null;
            m_ServiceCrew = null;
            m_ServiceResource = null;
        }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary("ServiceAppointment");
            ServiceAppointment = DeserializationUtils.GetSingleObjectReference<ServiceAppointment>(ServiceAppointmentId, objectJson, i_ReferenceResolver);
            if (ServiceAppointment != null && i_ShouldAddToRelatedLists)
            {
                ServiceAppointment.ServiceResources.Add(this);
            }

            objectJson = removeObjectJTokenFromAdditionalDictionary("ServiceCrew");
            ServiceCrew = DeserializationUtils.GetSingleObjectReference<ServiceCrew>(ServiceCrewId, objectJson, i_ReferenceResolver);

            objectJson = removeObjectJTokenFromAdditionalDictionary("ServiceResource");
            ServiceResource = DeserializationUtils.GetSingleObjectReference<ServiceResource>(ServiceResourceId, objectJson, i_ReferenceResolver);

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }

        internal override void removeReferencesBeforeDelete(SFRefereceResolver i_ReferenceResolver)
        {
            updateReferencesAfterDeserialize(i_ReferenceResolver, false);

            if (ServiceAppointment != null)
            {
                if (ServiceResource != null && ServiceResource.Equals(ServiceAppointment.ScheduledSTM.ServiceResource))
                {
                    ServiceAppointment.ScheduledSTM = null;
                }

                ServiceAppointment.ServiceResources.Remove(this);
            }

            base.removeReferencesBeforeDelete(i_ReferenceResolver);
        }
    }
}
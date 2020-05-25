using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.DataModel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class ServiceAppointment : sObject
    {
        [JsonIgnore]
        public TimeSpan? ActualDuration { get; set; }

        public DateTime? ActualStart { get; set; }

        public DateTime? ActualEndTime { get; set; }

        public DateTime? ArrivalWindowStartTime { get; set; }

        public DateTime? ArrivalWindowEndTime { get; set; }

        public string Description { get; set; }

        public DateTime? DueDate { get; set; }

        [JsonProperty("Duration")]
        private double? durationNumericValue
        {
            get { return NumericFields["Duration"]; }
            set { NumericFields["Duration"] = value; }
        }

        [JsonProperty]
        private string DurationType { get; set; }

        [JsonIgnore]
        public TimeSpan Duration
        {
            get
            {
                TimeSpan calculatedDuration;
                switch (DurationType)
                {
                    case "Minutes":
                        calculatedDuration = TimeSpan.FromMinutes(durationNumericValue.Value);
                        break;
                    case "Hours":
                        calculatedDuration = TimeSpan.FromHours(durationNumericValue.Value);
                        break;
                    default:
                        calculatedDuration = TimeSpan.Zero;
                        break;
                }


                return calculatedDuration;
            }
        }

        public DateTime? EarliestStartTime { get; set; }

        [JsonProperty("Latitude")]
        private double? internalLatitude
        {
            get { return NumericFields["Latitude"]; }
            set { NumericFields["Latitude"] = value; }
        }

        [JsonIgnore]
        public double? Latitude
        {
            get
            {
                return (internalLatitude.HasValue) ? internalLatitude : null;
            }
        }

        [JsonProperty("Longitude")]
        private double? internalLongitude
        {
            get { return NumericFields["Longitude"]; }
            set { NumericFields["Longitude"] = value; }
        }

        [JsonIgnore]
        public double? Longitude
        {
            get
            {
                return (internalLongitude.HasValue) ? internalLongitude : null;
            }
        }


        [JsonIgnore]
        public TimeZoneInfo TimeZone
        {
            get
            {
                return (ServiceTerritory == null) ? TimeZoneInfo.Utc : ServiceTerritory.TimeZone;
            }
        }

        [JsonProperty]
        private string ParentRecordId { get; set; }

        [JsonProperty]
        private string ParentRecordType { get; set; }

        [JsonIgnore]
        public IServiceAppoinmentParent ParentRecord { get; set; }

        public DateTime? SchedStartTime { get; set; }

        public DateTime? SchedEndTime { get; set; }

        public string Note { get; set; }

        [JsonProperty]
        private string ServiceTerritoryId { get; set; }

        [JsonIgnore]
        public ServiceTerritory ServiceTerritory { get; set; }

        public string Status { get; set; }

        public string Subject { get; set; }

        [JsonProperty]
        private string WorkTypeId { get; set; }

        [JsonIgnore]
        public WorkType WorkType { get; set; }

        [JsonProperty("ServiceResources")]
        private RelatedObjectCollection<AssignedResource> assignedResourcesCollection { get; set; }

        [JsonIgnore]
        public List<AssignedResource> ServiceResources { get; }

        //[JsonIgnore]
        //public ServiceResource Resource
        //{
        //    get { return ServiceResources?[0].ServiceResource; }
        //}

        [JsonIgnore]
        public AssignedResource AssignedServiceResource
        {
            get { return ServiceResources.FirstOrDefault(); }
        }

        [JsonIgnore]
        public bool IsScheduled
        {
            get
            {
                return SchedStartTime.HasValue && SchedEndTime.HasValue && ServiceResources.Count > 0;
            }
        }

        public ServiceAppointment() :
            this(null)
        { }

        public ServiceAppointment(string i_ObjectId) : base(i_ObjectId)
        {
            ServiceResources = new List<AssignedResource>();
        }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary("WorkType");
            WorkType = DeserializationUtils.GetSingleObjectReference<WorkType>(WorkTypeId, objectJson, i_ReferenceResolver);

            objectJson = removeObjectJTokenFromAdditionalDictionary("ServiceTerritory");
            ServiceTerritory = DeserializationUtils.GetSingleObjectReference<ServiceTerritory>(ServiceTerritoryId, objectJson, i_ReferenceResolver);

            objectJson = removeObjectJTokenFromAdditionalDictionary("ParentRecord");
            switch (ParentRecordType)
            {
                case "WorkOrder":
                    ParentRecord = DeserializationUtils.GetSingleObjectReference<WorkOrder>(ParentRecordId, objectJson, i_ReferenceResolver);
                    break;
                case "WorkOrderLineItem":
                    ParentRecord = DeserializationUtils.GetSingleObjectReference<WorkOrderLineItem>(ParentRecordId, objectJson, i_ReferenceResolver);
                    break;
            }

            DeserializationUtils.ProcessRelatedObjectsCollection(assignedResourcesCollection, i_ReferenceResolver);
            assignedResourcesCollection = null;

            Type myType = GetType();

            string propertyName = "Scheduling_Policy_Used__r";
            propertyName = DeserializationUtils.GetPropertyName(myType.GetProperty(propertyName), propertyName);
            objectJson = removeObjectJTokenFromAdditionalDictionary(propertyName);
            Scheduling_Policy_Used__r = DeserializationUtils.GetSingleObjectReference<Scheduling_Policy__c>(Scheduling_Policy_Used__c, objectJson, i_ReferenceResolver);

            propertyName = "Related_Service__r";
            propertyName = DeserializationUtils.GetPropertyName(myType.GetProperty(propertyName), propertyName);

            objectJson = removeObjectJTokenFromAdditionalDictionary(propertyName);
            Related_Service__r = DeserializationUtils.GetSingleObjectReference<ServiceAppointment>(Related_Service__c, objectJson, i_ReferenceResolver);

            if (IsScheduled)
                ScheduledSTM = ServiceResources.First().ServiceResource.GetServiceTerritoryMemberByDateTime(SchedStartTime.Value);
            
            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }

        public bool IsOverlapsHorizon(DateTime i_HorizonStart, DateTime? i_HorizonEnd = null)
        {
            if (!i_HorizonEnd.HasValue)
            {
                i_HorizonEnd = i_HorizonStart;
            }

            bool isOverlaping = i_HorizonEnd >= SchedStartTime;
            if (SchedEndTime.HasValue)
            {
                isOverlaping &= i_HorizonStart <= SchedEndTime.Value;
            }
            return isOverlaping;
        }

        public bool isScheduledOutOfHorizon(DateTime? i_HorizonStart = null, DateTime? i_HorizonEnd = null)
        {
            if(i_HorizonEnd == null && i_HorizonStart == null)
            {
                return false;
            }
            if (!i_HorizonEnd.HasValue)
            {
                i_HorizonEnd = i_HorizonStart;
            }
            return SchedEndTime < i_HorizonStart || SchedStartTime > i_HorizonEnd;
        }

    }
}

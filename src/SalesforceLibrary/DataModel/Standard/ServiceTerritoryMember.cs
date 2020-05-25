using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using System;
using System.Collections.Generic;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class ServiceTerritoryMember : ResourceTimePhasedSObject
    {
        [JsonProperty]
        private string OperatingHoursId { get; set; }

        [JsonIgnore]
        public OperatingHours OperatingHours { get; set; }

        [JsonProperty]
        private string ServiceTerritoryId { get; set; }

        [JsonIgnore]
        public ServiceTerritory ServiceTerritory { get; set; }

        public string TerritoryType { get; set; }

        [JsonProperty("Latitude")]
        private double? internalLatitude
        {
            get { return NumericFields["Latitude"]; }
            set { NumericFields["Latitude"] = value; }
        }

        [JsonIgnore]
        public bool AllowScheduleOnExtended { get; set; }

        [JsonIgnore]
        public double? Latitude
        {
            get
            {
                return (internalLatitude.HasValue) ? internalLatitude : ServiceTerritory?.Latitude;
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
                return (internalLongitude.HasValue) ? internalLongitude : ServiceTerritory?.Longitude;
            }
        }


        [JsonIgnore]
        public TimeZoneInfo TimeZone
        {
            get
            {
                return (OperatingHours == null) ? ServiceTerritory?.TimeZone : OperatingHours.TimeZone;
            }
        }

        [JsonIgnore]
        public List<ResourceCapacity> Capacities
        {
            get
            {
                return getCapacitiesForSTM();
            }
        }

        public ServiceTerritoryMember()
        {
        }

        public ServiceTerritoryMember(string i_ObjectId) : base(i_ObjectId)
        {
        }

        private List<ResourceCapacity> getCapacitiesForSTM()
        {
            List<ResourceCapacity> capacityList = new List<ResourceCapacity>();
            if (ServiceResource != null)
            {
                foreach (ResourceCapacity capacity in ServiceResource.Capacities)
                {
                    DateTime capacityStartTime = capacity.StartDate;
                    DateTime capacityEndTime = capacity.EndDate;
                    if ((capacityEndTime <= EffectiveEndDate || EffectiveEndDate == null) && (capacityStartTime >= EffectiveStartDate))
                    {
                        capacityList.Add(capacity);
                    }
                }
            }
            return capacityList;
        }


        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary("OperatingHours");
            OperatingHours = DeserializationUtils.GetSingleObjectReference<OperatingHours>(OperatingHoursId, objectJson, i_ReferenceResolver);

            objectJson = removeObjectJTokenFromAdditionalDictionary("ServiceResource");
            ServiceResource = DeserializationUtils.GetSingleObjectReference<ServiceResource>(ServiceResourceId, objectJson, i_ReferenceResolver);
            if (ServiceResource != null && !string.IsNullOrEmpty(TerritoryType))
            {
                ServiceResource.ServiceTerritories.Add(this);
            }

            objectJson = removeObjectJTokenFromAdditionalDictionary("ServiceTerritory"); ;
            ServiceTerritory = DeserializationUtils.GetSingleObjectReference<ServiceTerritory>(ServiceTerritoryId, objectJson, i_ReferenceResolver);

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}
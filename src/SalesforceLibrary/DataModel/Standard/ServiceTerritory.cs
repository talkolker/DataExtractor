using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using System;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class ServiceTerritory : sObject
    {
        public bool? IsActive
        {
            get { return BooleanFields["IsActive"]; }
            set { BooleanFields["IsActive"] = value; }
        }

        public double? Latitude
        {
            get { return NumericFields["Latitude"]; }
            set { NumericFields["Latitude"] = value; }
        }

        public double? Longitude
        {
            get { return NumericFields["Longitude"]; }
            set { NumericFields["Longitude"] = value; }
        }

        [JsonProperty] internal string OperatingHoursId { get; set; }

        [JsonIgnore]
        public OperatingHours OperatingHours { get; set; }

        [JsonProperty]
        private string ParentTerritoryId { get; }

        [JsonIgnore]
        public ServiceTerritory ParentTerritory { get; set; }

        [JsonProperty]
        private string TopLevelTerritoryId { get; }

        [JsonIgnore]
        public ServiceTerritory TopLevelTerritory { get; set; }

        public TimeZoneInfo TimeZone
        {
            get
            {
                return (OperatingHours == null) ? TimeZoneInfo.Utc : OperatingHours.TimeZone;
            }
        }

        public ServiceTerritory()
        {
        }

        public ServiceTerritory(string i_ObjectId) : base(i_ObjectId)
        {
        }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary("OperatingHours");
            OperatingHours = DeserializationUtils.GetSingleObjectReference<OperatingHours>(OperatingHoursId, objectJson, i_ReferenceResolver);

            objectJson = removeObjectJTokenFromAdditionalDictionary("ParentTerritory");
            ParentTerritory = DeserializationUtils.GetSingleObjectReference<ServiceTerritory>(ParentTerritoryId, objectJson, i_ReferenceResolver);

            objectJson = removeObjectJTokenFromAdditionalDictionary("TopLevelTerritory");
            TopLevelTerritory = DeserializationUtils.GetSingleObjectReference<ServiceTerritory>(TopLevelTerritoryId, objectJson, i_ReferenceResolver);

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}
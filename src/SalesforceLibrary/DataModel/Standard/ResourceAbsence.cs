using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using System;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class ResourceAbsence : sObjectWithRecordType
    {
        private ServiceResource m_Resource;

        public string Description { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }

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

        [JsonProperty]
        private string ResourceId { get; set; }

        [JsonIgnore]
        public ServiceResource Resource
        {
            get
            {
                return m_Resource;
            }
            set
            {
                m_Resource = value;
                ResourceId = m_Resource?.Id;
            }
        }

        public string Type { get; set; }

        public ResourceAbsence() :
            this(null)
        {
            m_Resource = null;
        }

        public ResourceAbsence(string i_ObjectId) : base(i_ObjectId)
        {
            m_Resource = null;
        }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary("Resource");
            Resource = DeserializationUtils.GetSingleObjectReference<ServiceResource>(ResourceId, objectJson, i_ReferenceResolver);

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);

            ScheduledSTM = Resource.GetServiceTerritoryMemberByDateTime(Start.Value);
        }

        public bool IsOverlapsHorizon(DateTime i_HorizonStart, DateTime? i_HorizonEnd = null)
        {
            if (!i_HorizonEnd.HasValue)
            {
                i_HorizonEnd = i_HorizonStart;
            }
            bool isOverlaping = i_HorizonEnd >= Start && i_HorizonStart <= End;
            return isOverlaping;
        }
    }
}
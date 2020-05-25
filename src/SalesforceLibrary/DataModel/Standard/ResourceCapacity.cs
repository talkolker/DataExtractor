using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using System;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class ResourceCapacity : sObject
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [JsonProperty]
        private double? CapacityInHours
        {
            get { return NumericFields["CapacityInHours"]; }
            set { NumericFields["CapacityInHours"] = value; }
        }

        public TimeSpan? Capacity
        {
            get
            {
                TimeSpan? calculatedValue = null;
                if (CapacityInHours.HasValue)
                {
                    calculatedValue = TimeSpan.FromHours(CapacityInHours.Value);
                }
                return calculatedValue;
            }
        }

        public string TimePeriod { get; set; }

        public double? CapacityInWorkItems
        {
            get { return NumericFields["CapacityInWorkItems"]; }
            set { NumericFields["CapacityInWorkItems"] = value; }
        }

        [JsonProperty]
        private string ServiceResourceId { get; set; }

        [JsonIgnore]
        public ServiceResource ServiceResource { get; set; }

        public ResourceCapacity() :
            this(null)
        { }

        public ResourceCapacity(string i_ObjectId) : base(i_ObjectId)
        { }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary("ServiceResource");
            ServiceResource = DeserializationUtils.GetSingleObjectReference<ServiceResource>(ServiceResourceId, objectJson, i_ReferenceResolver);
            if (ServiceResource != null)
            {
                ServiceResource.Capacities.Add(this);
            }

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}
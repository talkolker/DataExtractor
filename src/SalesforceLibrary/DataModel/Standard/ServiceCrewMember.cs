using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using System;

namespace SalesforceLibrary.DataModel.Standard
{

    [JsonObject]
    public partial class ServiceCrewMember : sObject
    {
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool? IsLeader
        {
            get { return BooleanFields["CapacityInWorkItems"]; }
            set { BooleanFields["CapacityInWorkItems"] = value; }
        }

        [JsonProperty]
        private string ServiceCrewId { get; set; }

        [JsonIgnore]
        public ServiceCrew ServiceCrew { get; set; }

        [JsonProperty]
        private string ServiceResourceId { get; set; }

        [JsonIgnore]
        public ServiceResource ServiceResource { get; set; }

        public ServiceCrewMember() :
            this(null)
        { }

        public ServiceCrewMember(string i_ObjectId) : base(i_ObjectId)
        {
        }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary("ServiceCrew");
            ServiceCrew = DeserializationUtils.GetSingleObjectReference<ServiceCrew>(ServiceCrewId, objectJson, i_ReferenceResolver);
            if (ServiceCrew != null)
            {
                ServiceCrew.ServiceCrewMembers.Add(this);
            }

            objectJson = removeObjectJTokenFromAdditionalDictionary("ServiceResource");
            ServiceResource = DeserializationUtils.GetSingleObjectReference<ServiceResource>(ServiceResourceId, objectJson, i_ReferenceResolver);
            if (ServiceResource != null)
            {
                ServiceResource.ServiceCrewMembers.Add(this);
            }

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class ResourcePreference : sObject
    {
        public string PreferenceType { get; set; }

        [JsonProperty]
        private string RelatedRecordId { get; set; }

        [JsonIgnore]
        public IResourcePreferenceParent RelatedRecord { get; set; }

        [JsonProperty]
        private string ServiceResourceId { get; set; }

        [JsonIgnore]
        public ServiceResource ServiceResource { get; set; }

        public ResourcePreference() :
            this(null)
        { }

        public ResourcePreference(string i_ObjectId) : base(i_ObjectId)
        { }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary("RelatedRecord");
            sObject resourcePreferenceParent = DeserializationUtils.GetSingleObjectReference<sObject>(RelatedRecordId, objectJson, i_ReferenceResolver);
            switch (resourcePreferenceParent.attributes.type)
            {
                case "WorkOrder":
                    RelatedRecord = (WorkOrder)resourcePreferenceParent;
                    break;
            }
            if (RelatedRecord != null)
            {
                RelatedRecord.ResourcePreferences.Add(this);
            }

            objectJson = removeObjectJTokenFromAdditionalDictionary("ServiceResource");
            ServiceResource = DeserializationUtils.GetSingleObjectReference<ServiceResource>(ServiceResourceId, objectJson, i_ReferenceResolver);

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}
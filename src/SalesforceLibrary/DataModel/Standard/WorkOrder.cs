using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using System;
using System.Collections.Generic;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class WorkOrder : sObject, IServiceAppoinmentParent, IResourcePreferenceParent, ISkillRequirementParent
    {
        public string Description { get; set; }

        [JsonIgnore]
        public TimeSpan? Duration { get; set; }

        public DateTime? EndDate { get; set; }

        public bool? IsGeneratedFromMaintenancePlan
        {
            get { return BooleanFields["IsGeneratedFromMaintenancePlan"]; }
            set { BooleanFields["IsGeneratedFromMaintenancePlan"] = value; }
        }

        public bool? IsClosed
        {
            get { return BooleanFields["IsClosed"]; }
            set { BooleanFields["IsClosed"] = value; }
        }

        public int? MinimumCrewSize
        {
            get
            {
                double? tempValue = NumericFields["MinimumCrewSize"];
                if (tempValue.HasValue)
                {
                    return Convert.ToInt32(tempValue.Value);
                }
                return null;
            }
            set { NumericFields["MinimumCrewSize"] = value; }
        }

        [JsonProperty]
        private string ParentWorkOrderId { get; set; }

        [JsonIgnore]
        public WorkOrder ParentWorkOrder { get; set; }

        public int? RecommendedCrewSize
        {
            get
            {
                double? tempValue = NumericFields["RecommendedCrewSize"];
                if (tempValue.HasValue)
                {
                    return Convert.ToInt32(tempValue.Value);
                }
                return null;
            }
            set { NumericFields["RecommendedCrewSize"] = value; }
        }

        [JsonProperty]
        private string RootWorkOrderId { get; set; }

        [JsonIgnore]
        public WorkOrder RootWorkOrder { get; set; }

        [JsonProperty]
        private string ServiceTerritoryId { get; set; }

        [JsonIgnore]
        public ServiceTerritory ServiceTerritory { get; set; }

        public DateTime? StartDate { get; set; }

        public string Status { get; set; }

        public string Subject { get; set; }

        [JsonProperty]
        private string WorkTypeId { get; set; }

        [JsonIgnore]
        public WorkType WorkType { get; set; }

        [JsonProperty("SkillRequirements")]
        private RelatedObjectCollection<SkillRequirement> skillRequirementsCollection { get; set; }

        [JsonIgnore]
        public List<SkillRequirement> SkillRequirements { get; set; }

        [JsonProperty("ResourcePreferences")]
        private RelatedObjectCollection<ResourcePreference> resourcePreferencesCollection { get; set; }

        [JsonIgnore]
        public List<ResourcePreference> ResourcePreferences { get; set; }
        

        public WorkOrder() : this(null)
        {
        }

        public WorkOrder(string i_ObjectId) : base(i_ObjectId)
        {
            SkillRequirements = new List<SkillRequirement>();
            ResourcePreferences = new List<ResourcePreference>();
        }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary("ServiceTerritory");
            ServiceTerritory = DeserializationUtils.GetSingleObjectReference<ServiceTerritory>(ServiceTerritoryId, objectJson, i_ReferenceResolver);

            objectJson = removeObjectJTokenFromAdditionalDictionary("ParentWorkOrder");
            ParentWorkOrder = DeserializationUtils.GetSingleObjectReference<WorkOrder>(ParentWorkOrderId, objectJson, i_ReferenceResolver);

            objectJson = removeObjectJTokenFromAdditionalDictionary("RootWorkOrder");
            RootWorkOrder = DeserializationUtils.GetSingleObjectReference<WorkOrder>(RootWorkOrderId, objectJson, i_ReferenceResolver);

            objectJson = removeObjectJTokenFromAdditionalDictionary("WorkType");
            WorkType = DeserializationUtils.GetSingleObjectReference<WorkType>(WorkTypeId, objectJson, i_ReferenceResolver);

            VisitingHours__r = DeserializationUtils.GetSingleObjectReference<OperatingHours>(VisitingHours__c, i_ReferenceResolver);
            VisitingHours__r = DeserializationUtils.CreateEmptyReferenceFromId(VisitingHours__c, VisitingHours__r, i_ReferenceResolver);

            DeserializationUtils.ProcessRelatedObjectsCollection(skillRequirementsCollection, i_ReferenceResolver);
            skillRequirementsCollection = null;

            DeserializationUtils.ProcessRelatedObjectsCollection(resourcePreferencesCollection, i_ReferenceResolver);
            resourcePreferencesCollection = null;

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}
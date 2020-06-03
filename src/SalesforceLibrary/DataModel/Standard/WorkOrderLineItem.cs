using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using System;
using System.Collections.Generic;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class WorkOrderLineItem : ServiceParent, IServiceAppoinmentParent
    {
        public string Description { get; set; }

        [JsonIgnore]
        public TimeSpan? Duration { get; set; }

        public DateTime? EndDate { get; set; }

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
        private string ParentWorkOrderLineItemId { get; set; }

        [JsonIgnore]
        public WorkOrderLineItem ParentWorkOrderLineItem { get; set; }

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
        private string RootWorkOrderLineItemId { get; set; }

        [JsonIgnore]
        public WorkOrderLineItem RootWorkOrderLineItem { get; set; }

        [JsonProperty]
        private string ServiceTerritoryId { get; set; }

        [JsonIgnore]
        public ServiceTerritory ServiceTerritory { get; set; }

        public DateTime? StartDate { get; set; }

        public string Status { get; set; }

        public string Subject { get; set; }

        [JsonProperty]
        private string WorkOrderId { get; set; }

        [JsonIgnore]
        public WorkOrder WorkOrder { get; set; }

        [JsonProperty]
        private string WorkTypeId { get; set; }

        [JsonIgnore]
        public WorkType WorkType { get; set; }

        [JsonProperty("SkillRequirements")]
        private RelatedObjectCollection<SkillRequirement> skillRequirementsCollection { get; set; }

        //[JsonIgnore]
        //public List<SkillRequirement> SkillRequirements { get; set; }

        public WorkOrderLineItem(): this(null)
        {
        }

        public WorkOrderLineItem(string i_ObjectId) : base(i_ObjectId) { }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary("ServiceTerritory");
            ServiceTerritory = DeserializationUtils.GetSingleObjectReference<ServiceTerritory>(ServiceTerritoryId, objectJson, i_ReferenceResolver);

            objectJson = removeObjectJTokenFromAdditionalDictionary("ParentWorkOrderLineItem");
            ParentWorkOrderLineItem = DeserializationUtils.GetSingleObjectReference<WorkOrderLineItem>(ParentWorkOrderLineItemId, objectJson, i_ReferenceResolver);

            objectJson = removeObjectJTokenFromAdditionalDictionary("RootWorkOrderLineItem");
            RootWorkOrderLineItem = DeserializationUtils.GetSingleObjectReference<WorkOrderLineItem>(RootWorkOrderLineItemId, objectJson, i_ReferenceResolver);

            objectJson = removeObjectJTokenFromAdditionalDictionary("WorkOrder");
            WorkOrder = DeserializationUtils.GetSingleObjectReference<WorkOrder>(WorkOrderId, objectJson, i_ReferenceResolver);

            objectJson = removeObjectJTokenFromAdditionalDictionary("WorkType");
            WorkType = DeserializationUtils.GetSingleObjectReference<WorkType>(WorkTypeId, objectJson, i_ReferenceResolver);

            VisitingHours__r = DeserializationUtils.GetSingleObjectReference<OperatingHours>(VisitingHours__c, i_ReferenceResolver);
            VisitingHours__r = DeserializationUtils.CreateEmptyReferenceFromId(VisitingHours__c, VisitingHours__r, i_ReferenceResolver);

            DeserializationUtils.ProcessRelatedObjectsCollection(skillRequirementsCollection, i_ReferenceResolver);
            skillRequirementsCollection = null;

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}
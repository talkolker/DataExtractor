using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class SkillRequirement : sObject
    {
        [JsonProperty]
        private string RelatedRecordId { get; set; }

        [JsonIgnore]
        public ServiceParent RelatedRecord { get; set; }

        [JsonProperty] internal string SkillId { get; set; }

        [JsonIgnore]
        public Skill Skill { get; set; }

        public double? SkillLevel
        {
            get { return NumericFields["SkillLevel"]; }
            set { NumericFields["SkillLevel"] = value; }
        }


        public SkillRequirement()
        {
        }

        public SkillRequirement(string i_ObjectId) : base(i_ObjectId)
        {
        }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary("RelatedRecord");
            sObject skillRequirementParent = DeserializationUtils.GetSingleObjectReference<sObject>(RelatedRecordId, objectJson, i_ReferenceResolver);
            switch (skillRequirementParent.attributes.type)
            {
                case "WorkOrder":
                    RelatedRecord = (WorkOrder)skillRequirementParent;
                    break;
                case "WorkOrderLineItem":
                    RelatedRecord = (WorkOrderLineItem)skillRequirementParent;
                    break;
                case "WorkType":
                    RelatedRecord = (WorkType)skillRequirementParent;
                    break;
            }
            if (RelatedRecord != null)
            {
                RelatedRecord.SkillRequirements.Add(this);
            }

            objectJson = removeObjectJTokenFromAdditionalDictionary("Skill");
            Skill = DeserializationUtils.GetSingleObjectReference<Skill>(SkillId, objectJson, i_ReferenceResolver);
            Skill = DeserializationUtils.CreateEmptyReferenceFromId(SkillId, Skill, i_ReferenceResolver);

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}
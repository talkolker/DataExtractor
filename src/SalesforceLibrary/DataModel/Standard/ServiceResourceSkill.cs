using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using System;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class ServiceResourceSkill : ResourceTimePhasedSObject
    {
        [JsonProperty] internal string SkillId { get; set; }

        [JsonIgnore]
        public Skill Skill { get; set; }

        public double? SkillLevel
        {
            get { return NumericFields["SkillLevel"]; }
            set { NumericFields["SkillLevel"] = value; }
        }

        public ServiceResourceSkill() :
            this(i_ObjectId: null)
        { }

        public ServiceResourceSkill(string i_ObjectId) : base(i_ObjectId)
        { }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary("ServiceResource");
            ServiceResource = DeserializationUtils.GetSingleObjectReference<ServiceResource>(ServiceResourceId, objectJson, i_ReferenceResolver);

            if (ServiceResource != null)
            {
                ServiceResource.Skills.Add(this);
            }

            objectJson = removeObjectJTokenFromAdditionalDictionary("Skill");
            Skill = DeserializationUtils.GetSingleObjectReference<Skill>(SkillId, objectJson, i_ReferenceResolver);
            Skill = DeserializationUtils.CreateEmptyReferenceFromId(SkillId, Skill, i_ReferenceResolver);

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}
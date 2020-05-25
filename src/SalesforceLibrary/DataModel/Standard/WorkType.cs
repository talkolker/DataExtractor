using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Abstraction;
using System;
using System.Collections.Generic;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class WorkType : sObject, ISkillRequirementParent
    {
        [JsonProperty("ShouldAutoCreateSvcAppt")]
        public bool? ShouldAutoCreateServiceAppointment
        {
            get { return BooleanFields["ShouldAutoCreateSvcAppt"]; }
            set { BooleanFields["ShouldAutoCreateSvcAppt"] = value; }
        }

        public string Description { get; set; }

        [JsonIgnore]
        public TimeSpan? EstimatedDuration { get; set; }

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

        [JsonProperty("SkillRequirements")]
        private RelatedObjectCollection<SkillRequirement> skillRequirementsCollection { get; set; }

        [JsonIgnore]
        public List<SkillRequirement> SkillRequirements { get; set; }

        public WorkType()
        {
        }

        public WorkType(string i_ObjectId) : base(i_ObjectId)
        {
        }
    }
}
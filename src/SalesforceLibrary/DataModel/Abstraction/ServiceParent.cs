using System.Collections.Generic;
using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Standard;

namespace SalesforceLibrary.DataModel.Abstraction
{
    [JsonObject]
    public class ServiceParent : sObject
    {
        [JsonIgnore]
        public List<SkillRequirement> SkillRequirements { get; set; }
        
        [JsonProperty("SkillRequirements")]
        internal RelatedObjectCollection<SkillRequirement> skillRequirementsCollection { get; set; }
        
        [JsonProperty]
        internal string VisitingHours__c { get; set; }
        
        public ServiceParent(string i_ObjectId) : base(i_ObjectId)
        {
            SkillRequirements = new List<SkillRequirement>();
        }

        public ServiceParent(){ }
    }
}
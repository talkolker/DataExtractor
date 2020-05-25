using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Standard;
using System.Collections.Generic;

namespace SalesforceLibrary.DataModel.Abstraction
{
    public interface ISkillRequirementParent
    {
        List<SkillRequirement> SkillRequirements { get; set; }
    }
}

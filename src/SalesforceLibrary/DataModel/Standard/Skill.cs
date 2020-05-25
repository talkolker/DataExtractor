using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Abstraction;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class Skill : sObject
    {
        public Skill()
        {
        }

        public Skill(string i_ObjectId) : base(i_ObjectId)
        {
        }
    }
}
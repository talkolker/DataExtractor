using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Abstraction;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public class PermissionSetAssignment : sObject
    {
        [JsonProperty]
        public string AssigneeId { get; set; }
    }
}
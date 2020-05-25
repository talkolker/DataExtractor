using Newtonsoft.Json;

namespace SalesforceLibrary.DataModel.FSL
{
    [JsonObject]
    public class SupportHelper
    {
        public bool HasObjectsToScheduleInRestAPI { get; set; }
    }
}

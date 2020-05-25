using Newtonsoft.Json;
using System.Collections.Generic;

namespace SalesforceLibrary.DataModel.Abstraction
{
    [JsonObject]
    internal class RelatedObjectCollection<T> where T : sObject
    {
        [JsonProperty]
        internal int totalSize { get; set; }

        [JsonProperty]
        internal bool done { get; set; }

        [JsonProperty]
        internal List<T> records { get; set; }
        
        //[JsonProperty]
        //internal string DeveloperName { get; set; }
    }
}

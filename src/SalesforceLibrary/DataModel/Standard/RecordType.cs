using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Abstraction;
using System;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public class RecordType : sObject
    {
        public RecordType(string Id, string DeveloperName) : base(Id)
        {
            Name = DeveloperName;
        }
    }
}

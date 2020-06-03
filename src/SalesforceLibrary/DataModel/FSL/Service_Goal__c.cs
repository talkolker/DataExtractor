using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Abstraction;

namespace SalesforceLibrary.DataModel.FSL
{
    [JsonObject]
    public class Service_Goal__c : sObjectWithRecordType
    {
        [PackageNamespace("FSL")]
        public bool? Ignore_Home_Base_Coordinates__c
        {
            get { return BooleanFields["FSL__Ignore_Home_Base_Coordinates__c"]; }
            set { BooleanFields["FSL__Ignore_Home_Base_Coordinates__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public string Prioritize_Resource__c { get; set; }

        [PackageNamespace("FSL")]
        public string Object_Group_Field__c { get; set; }

        [PackageNamespace("FSL")]
        public string Resource_Group_Field__c { get; set; }

        [PackageNamespace("FSL")]
        public string Resource_Priority_Field__c { get; set; }

        [PackageNamespace("FSL")]
        public string Custom_Type__c { get; set; }

        [PackageNamespace("FSL")]
        public string Custom_Logic_Data__c { get; set; }
        
        public string DeveloperName { get; set; }
        
        public int? Weight__c { get; set; }

        public Service_Goal__c() :
            this(null)
        { }

        public Service_Goal__c(string i_ObjectId) :
            base(i_ObjectId)
        { }
    }
}

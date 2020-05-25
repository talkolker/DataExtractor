using SalesforceLibrary.DataModel.Abstraction;
using Newtonsoft.Json;

namespace SalesforceLibrary.DataModel.FSL
{
    [JsonObject]
    public class Geocode_Settings__c : sObject
    {

        [PackageNamespace("FSL")]
        public bool Alert_On_Callout_Failure__c { get; set; }

        [PackageNamespace("FSL")]
        public string Token__c { get; set; }

        [PackageNamespace("FSL")]
        public bool Is_Staging_Server__c { get; set; }

        [PackageNamespace("FSL")]
        public bool Is_Edge_SLR__c { get; set; }

        [PackageNamespace("FSL")]
        public string Prod_URL__c { get; set; }

        [PackageNamespace("FSL")]
        public string Prod_Custom_URL__c { get; set; }

        [PackageNamespace("FSL")]
        public double Batch_Size_For_SLR_Callouts__c { get; set; }

        [PackageNamespace("FSL")]
        public double Aerial_Fallback__c { get; set; }

    }
}

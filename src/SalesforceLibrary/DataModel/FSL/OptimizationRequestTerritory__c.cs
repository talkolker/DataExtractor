using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Abstraction;
using System;

namespace SalesforceLibrary.DataModel.FSL
{
    [JsonObject]
    public class OptimizationRequestTerritory__c : sObject {
        [PackageNamespace("FSL")]
        public string OptimizationRequestId__c { get; set; }
        [PackageNamespace("FSL")]
        public string ServiceTerritory__c { get; set; }
        [PackageNamespace("FSL")]
        public double TotalTravelTimeBefore__c { get; set; }
        [PackageNamespace("FSL")]
        public double TotalTravelTimeAfter__c { get; set; }
        [PackageNamespace("FSL")]
        public double TotalTravelTimeWithoutHomeBefore__c { get; set; }
        [PackageNamespace("FSL")]
        public double TotalTravelTimeWithoutHomeAfter__c { get; set; }
        [PackageNamespace("FSL")]
        public double AvgTravelTimeBefore__c { get; set; }
        [PackageNamespace("FSL")]
        public double AvgTravelTimeAfter__c { get; set; }
        [PackageNamespace("FSL")]
        public double AvgTravelTimeWithoutHomeBefore__c { get; set; }
        [PackageNamespace("FSL")]
        public double AvgTravelTimeWithoutHomeAfter__c { get; set; }
        [PackageNamespace("FSL")]
        public double TotalWorkDurationScheduledBefore__c { get; set; }
        [PackageNamespace("FSL")]
        public double TotalWorkDurationScheduledAfter__c { get; set; }
        [PackageNamespace("FSL")]
        public double UtilizationTotalBefore__c { get; set; }
        [PackageNamespace("FSL")]
        public double UtilizationTotalAfter__c { get; set; }
        [PackageNamespace("FSL")]
        public double UtilizationWrenchBefore__c { get; set; }
        [PackageNamespace("FSL")]
        public double UtilizationWrenchAfter__c { get; set; }
        [PackageNamespace("FSL")]
        public double TotalNumberOfScheduledServicesBefore__c { get; set; }
        [PackageNamespace("FSL")]
        public double TotalNumberOfScheduledServicesAfter__c { get; set; }
        [PackageNamespace("FSL")]
        public double TotalNumberOfOptimizableServices__c { get; set; }
        [PackageNamespace("FSL")]
        public double TotalNumberOfResources__c { get; set; }
        [PackageNamespace("FSL")]
        public double TotalNumberOfCrews__c { get; set; }
        [PackageNamespace("FSL")]
        public double TotalNumberOfCapacityBasedResources__c { get; set; }
        [PackageNamespace("FSL")]
        public double TotalWorkCapacity__c { get; set; }


    }
}

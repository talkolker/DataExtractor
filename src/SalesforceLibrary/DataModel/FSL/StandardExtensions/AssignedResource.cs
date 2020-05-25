using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SalesforceLibrary.DataModel.Standard
{
    public partial class AssignedResource
    {
        [PackageNamespace("FSL")]
        public double? EstimatedTravelDistanceFrom__c
        {
            get { return NumericFields["FSL__EstimatedTravelDistanceFrom__c"]; }
            set { NumericFields["FSL__EstimatedTravelDistanceFrom__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public double? EstimatedTravelDistanceTo__c
        {
            get { return NumericFields["FSL__EstimatedTravelDistanceTo__c"]; }
            set { NumericFields["FSL__EstimatedTravelDistanceTo__c"] = value; }
        }

        [PackageNamespace("FSL")]
        [JsonProperty("EstimatedTravelTimeFrom__c")]
        public double? estimatedTravelTimeFromFieldInMinutes
        {
            get { return NumericFields["FSL__EstimatedTravelTimeFrom__c"]; }
            set { NumericFields["FSL__EstimatedTravelTimeFrom__c"] = value; }
        }

        [JsonIgnore]
        public TimeSpan? EstimatedTravelTimeFrom__c
        {
            get
            {
                TimeSpan? convertedValue = null;
                if (estimatedTravelTimeFromFieldInMinutes.HasValue)
                {
                    convertedValue = TimeSpan.FromMinutes(estimatedTravelTimeFromFieldInMinutes.Value);
                }
                return convertedValue;
            }
            set
            {
                double? fieldValue = null;
                if (value.HasValue)
                {
                    fieldValue = Math.Ceiling(value.Value.TotalMinutes);
                }
                estimatedTravelTimeFromFieldInMinutes = fieldValue;
            }
        }

        [PackageNamespace("FSL")]
        public string Estimated_Travel_Time_From_Source__c { get; set; }

        [PackageNamespace("FSL")]
        public string Estimated_Travel_Time_To_Source__c { get; set; }

        [PackageNamespace("FSL")]
        public string Travel_Calculation_Method__c { get; set; }

        [PackageNamespace("FSL")]
        public bool? UpdatedByOptimization__c
        {
            get { return BooleanFields["FSL__UpdatedByOptimization__c"]; }
            set { BooleanFields["FSL__UpdatedByOptimization__c"] = value; }
        }
    }
}

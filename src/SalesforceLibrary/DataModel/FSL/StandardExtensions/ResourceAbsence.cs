using Newtonsoft.Json;
using System;

namespace SalesforceLibrary.DataModel.Standard
{
    public partial class ResourceAbsence
    {
        [PackageNamespace("FSL")]
        public bool? Approved__c
        {
            get { return BooleanFields["FSL__Approved__c"]; }
            set { BooleanFields["FSL__Approved__c"] = value; }
        }

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
        [JsonProperty("EstTravelTime__c")]
        public double? estimatedTravelTimeToFieldInMinutes
        {
            get { return NumericFields["FSL__EstTravelTime__c"]; }
            set { NumericFields["FSL__EstTravelTime__c"] = value; }
        }

        [JsonIgnore]
        public TimeSpan? EstTravelTime__c
        {
            get
            {
                TimeSpan? convertedValue = null;
                if (estimatedTravelTimeToFieldInMinutes.HasValue)
                {
                    convertedValue = TimeSpan.FromMinutes(estimatedTravelTimeToFieldInMinutes.Value);
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
                estimatedTravelTimeToFieldInMinutes = fieldValue;
            }
        }

        [PackageNamespace("FSL")]
        [JsonProperty("EstTravelTimeFrom__c")]
        public double? estimatedTravelTimeFromFieldInMinutes
        {
            get { return NumericFields["FSL__EstTravelTimeFrom__c"]; }
            set { NumericFields["FSL__EstTravelTimeFrom__c"] = value; }
        }

        [JsonIgnore]
        public TimeSpan? EstTravelTimeFrom__c
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

        [JsonIgnore]
        public ServiceTerritoryMember ScheduledSTM { get; set; }
    }
}

using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Abstraction;
using System;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.DataModel.Utils;

namespace SalesforceLibrary.DataModel.FSL
{
    [JsonObject]
    public class Work_Rule__c : sObjectWithRecordType
    {
        [PackageNamespace("FSL")]
        public string Boolean_Operator__c { get; set; }

        [PackageNamespace("FSL")]
        [JsonProperty("Break_Duration__c")]
        private double? breakDurationInMinutes
        {
            get { return NumericFields["FSL__Break_Duration__c"]; }
            set { NumericFields["FSL__Break_Duration__c"] = value; }
        }

        [JsonIgnore]
        public TimeSpan? Break_Duration__c
        {
            get
            {
                TimeSpan? calculatedValue = null;
                if (breakDurationInMinutes.HasValue)
                {
                    calculatedValue = TimeSpan.FromMinutes(breakDurationInMinutes.Value);
                }
                return calculatedValue;
            }
        }

        [PackageNamespace("FSL")]
        [JsonProperty("Break_Start__c")]
        private string breakStartString { get; set; }

        [JsonIgnore]
        public TimeSpan? Break_Start__c
        {
            get
            {
                TimeSpan? calculatedValue = null;
                if (!string.IsNullOrEmpty(breakStartString))
                {
                    calculatedValue = TimeSpan.Parse(breakStartString);
                }
                return calculatedValue;
            }
        }

        [PackageNamespace("FSL")]
        public bool? Crew_Resources_Availability__c
        {
            get { return BooleanFields["FSL__Crew_Resources_Availability__c"]; }
            set { BooleanFields["FSL__Crew_Resources_Availability__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public string Description__c { get; set; }

        [PackageNamespace("FSL")]
        public bool? Enable_Overtime__c
        {
            get { return BooleanFields["FSL__Enable_Overtime__c"]; }
            set { BooleanFields["FSL__Enable_Overtime__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Is_Fixed_Gap__c
        {
            get { return BooleanFields["FSL__Is_Fixed_Gap__c"]; }
            set { BooleanFields["FSL__Is_Fixed_Gap__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Match_Skill_Level__c
        {
            get { return BooleanFields["FSL__Match_Skill_Level__c"]; }
            set { BooleanFields["FSL__Match_Skill_Level__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public double? Maximum_Travel_From_Home__c
        {
            get { return NumericFields["FSL__Maximum_Travel_From_Home__c"]; }
            set { NumericFields["FSL__Maximum_Travel_From_Home__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public string Maximum_Travel_From_Home_Type__c { get; set; }

        [PackageNamespace("FSL")]
        public double? Minimum_Gap__c
        {
            get { return NumericFields["FSL__Minimum_Gap__c"]; }
            set { NumericFields["FSL__Minimum_Gap__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public string Object_Group_Field__c { get; set; }

        [PackageNamespace("FSL")]
        public bool? Pass_Empty_Values__c
        {
            get { return BooleanFields["FSL__Pass_Empty_Values__c"]; }
            set { BooleanFields["FSL__Pass_Empty_Values__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public string Resource_Group_Field__c { get; set; }

        [PackageNamespace("FSL")]
        public string Resource_Property__c { get; set; }

        [PackageNamespace("FSL")]
        public string Service_Property__c { get; set; }

        [PackageNamespace("FSL")]
        public string Service_Schedule_Time_Property__c { get; set; }

        [PackageNamespace("FSL")]
        public string Service_Time_Operator__c { get; set; }

        [PackageNamespace("FSL")]
        public string Service_Time_Property__c { get; set; }

        [PackageNamespace("FSL")]
        [JsonProperty("Start_of_Day__c")]
        private string startOfDayString { get; set; }


        [PackageNamespace("FSL")]
        public string CountType__c { get; set; }

        [PackageNamespace("FSL")]
        public string CountObject__c { get; set; }

        [PackageNamespace("FSL")]
        public string CountTimeResolution__c { get; set; }

        [PackageNamespace("FSL")]
        public string CustomFieldName__c { get; set; }

        [PackageNamespace("FSL")]
        public double? DefaultLimit__c { get; set; }
        
        [PackageNamespace("FSL")]
        public bool? isTimephased__c { get; set; }
        
        [PackageNamespace("FSL")]
        public string Service_Linking_Object_Reference_Field__c { get; set; }
        
        [PackageNamespace("FSL")]
        public string Service_Appointment_Matching_Field__c { get; set; }
        
        [PackageNamespace("FSL")]
        public string 	WorkRule_Start_DateTime_Field__c { get; set; }
        
        [PackageNamespace("FSL")]
        public string WorkRule_End_DateTime_Field__c { get; set; }
        
        [PackageNamespace("FSL")]
        public string Enhanced_Match_Linking_Object__c { get; set; }
        
        public string DeveloperName { get; set; }

        [JsonIgnore]
        public TimeSpan? Start_of_Day__c
        {
            get
            {
                TimeSpan? calculatedValue = null;
                if (!string.IsNullOrEmpty(startOfDayString))
                {
                    calculatedValue = TimeSpan.Parse(startOfDayString);
                }
                return calculatedValue;
            }
        }

        [PackageNamespace("FSL")]
        public double? Travel_From_Home__c
        {
            get { return NumericFields["FSL__Travel_From_Home__c"]; }
            set { NumericFields["FSL__Travel_From_Home__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public double? Travel_To_Home__c
        {
            get { return NumericFields["FSL__Travel_To_Home__c"]; }
            set { NumericFields["FSL__Travel_To_Home__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Match_Constant_Boolean_Value__c
        {
            get { return BooleanFields["FSL__Match_Constant_Boolean_Value__c"]; }
            set { BooleanFields["FSL__Match_Constant_Boolean_Value__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Working_Location_Enable_Primary__c
        {
            get { return BooleanFields["FSL__Working_Location_Enable_Primary__c"]; }
            set { BooleanFields["FSL__Working_Location_Enable_Primary__c"] = value; }
        }

        public Work_Rule__c() :
            this(null)
        { }

        public Work_Rule__c(string i_ObjectId) :
            base(i_ObjectId)
        { }

    }
}

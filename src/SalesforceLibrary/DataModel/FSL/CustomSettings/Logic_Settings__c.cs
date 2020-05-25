using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Abstraction;
using System;

namespace SalesforceLibrary.DataModel.FSL
{
    [JsonObject]
    public class Logic_Settings__c : sObject
    {
        [PackageNamespace("FSL")]
        public DayOfWeek? Default_First_Day_Of_Working_Week__c { get; set; }

        [PackageNamespace("FSL")]
        public string WO_Priority_Field__c { get; set; }

        [PackageNamespace("FSL")]
        public string WOLI_Priority_Field__c { get; set; }

        [PackageNamespace("FSL")]
        public string MDT_Boolean_Field__c { get; set; }

        [PackageNamespace("FSL")]
        public bool? Use_New_MST_Data_Model__c
        {
            get { return BooleanFields["FSL__Use_New_MST_Data_Model__c"]; }
            set { BooleanFields["FSL__Use_New_MST_Data_Model__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Approved_Absences__c
        {
            get { return BooleanFields["FSL__Approved_Absences__c"]; }
            set { BooleanFields["FSL__Approved_Absences__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public string Travel_Speed_Unit__c { get; set; }

        [PackageNamespace("FSL")]
        public bool? Enable_Start_Of_Day__c
        {
            get { return BooleanFields["FSL__Enable_Start_Of_Day__c"]; }
            set { BooleanFields["FSL__Enable_Start_Of_Day__c"] = value; }
        }

        [JsonIgnore]
        public bool IsSecondaryCalendarEnabled { get { return Include_Secondary_Calendar__c.HasValue && Include_Secondary_Calendar__c.Value; } }

        [PackageNamespace("FSL")]
        public bool? Include_Secondary_Calendar__c
        {
            get { return BooleanFields["FSL__Include_Secondary_Calendar__c"]; }
            set { BooleanFields["FSL__Include_Secondary_Calendar__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public double? Travel_Speed__c
        {
            get { return NumericFields["FSL__Travel_Speed__c"]; }
            set { NumericFields["FSL__Travel_Speed__c"] = value; }
        }

        [PackageNamespace("FSL")]
        [JsonProperty("Fail_On_Schedule__c")]
        public bool? AllOrNothing
        {
            get { return BooleanFields["FSL__Fail_On_Schedule__c"]; }
            set { BooleanFields["FSL__Fail_On_Schedule__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Enable_Crew_Members_Skill_Aggregation__c
        {
            get { return BooleanFields["FSL__Enable_Crew_Members_Skill_Aggregation__c"]; }
            set { BooleanFields["FSL__Enable_Crew_Members_Skill_Aggregation__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Use_SLR__c
        {
            get { return BooleanFields["FSL__Use_SLR__c"]; }
            set { BooleanFields["FSL__Use_SLR__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Use_Predictive__c
        {
            get { return BooleanFields["FSL__Use_Predictive__c"]; }
            set { BooleanFields["FSL__Use_Predictive__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public bool? Enable_Optimization_Insights__c
        {
            get { return BooleanFields["FSL__Enable_Optimization_Insights__c"]; }
            set { BooleanFields["FSL__Enable_Optimization_Insights__c"] = value; }
        }

        [JsonIgnore]
        public bool UseNewMSTDataModel
        {
            get
            {
                return Use_New_MST_Data_Model__c.HasValue &&
                       Use_New_MST_Data_Model__c.Value;
            }
        }

        [JsonIgnore]
        public bool UseMDTRules { get; set; }

        [JsonIgnore]
        public bool UseTimePhasedWorkingLocationRule { get; set; }

        [JsonIgnore]
        public bool UseTimePhasedSkillsRule { get; set; }

        [JsonIgnore]
        public bool RequiredResourceRuleEnabled { get; set; }
    }
}
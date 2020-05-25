using System;

namespace SalesforceLibrary.DataModel.Standard
{
    public partial class ServiceResource
    {
        [PackageNamespace("FSL")]
        public double? Efficiency__c
        {
            get { return NumericFields["FSL__Efficiency__c"]; }
            set { NumericFields["FSL__Efficiency__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public int? Priority__c
        {
            get
            {
                double? tempValue = NumericFields["FSL__Priority__c"];
                if (tempValue.HasValue)
                {
                    return Convert.ToInt32(tempValue.Value);
                }
                return null;
            }
            set { NumericFields["FSL__Priority__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public int? Travel_Speed__c
        {
            get
            {
                double? tempValue = NumericFields["FSL__Travel_Speed__c"];
                if (tempValue.HasValue)
                {
                    return Convert.ToInt32(tempValue.Value);
                }
                return null;
            }
            set { NumericFields["FSL__Travel_Speed__c"] = value; }
        }
    }
}

using System;

namespace SalesforceLibrary.DataModel.Standard
{
    public partial class ResourceCapacity
    {
        [PackageNamespace("FSL")]
        public int? MinutesUsed__c
        {
            get
            {
                double? tempValue = NumericFields["FSL__MinutesUsed__c"];
                if (tempValue.HasValue)
                {
                    return Convert.ToInt32(tempValue.Value);
                }
                return null;
            }
            set { NumericFields["FSL__MinutesUsed__c"] = value; }
        }

        [PackageNamespace("FSL")]
        public int? Work_Items_Allocated__c
        {
            get
            {
                double? tempValue = NumericFields["FSL__Work_Items_Allocated__c"];
                if (tempValue.HasValue)
                {
                    return Convert.ToInt32(tempValue.Value);
                }
                return null;
            }
            set { NumericFields["FSL__Work_Items_Allocated__c"] = value; }
        }
    }
}

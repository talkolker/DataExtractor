using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.DataModel.Utils;

namespace SalesforceLibrary.REST.FSL.OaaS
{
    public class InsightsKPIData
    {
        public string kpiData { get; set; }

        public string dataID { get; set; }

        public string optimizationSumKPIs { get; set; }

        public InsightsKPIData(Dictionary<string, OptimizationRequestTerritory__c> i_KPIs, OptimizationRequest__c i_OR, bool i_IsManaged = true)
        {
            if (!i_IsManaged)
            {
                DeserializationUtils.NamespacesToIgnore.Add("FSL");
            }
            else
            {
                DeserializationUtils.NamespacesToIgnore.Remove("FSL");
            }

            kpiData = JsonConvert.SerializeObject(i_KPIs, DeserializationUtils.SFJsonSerializerSettings);
            optimizationSumKPIs = JsonConvert.SerializeObject(i_OR, DeserializationUtils.SFJsonSerializerSettings);
        }
    }
}

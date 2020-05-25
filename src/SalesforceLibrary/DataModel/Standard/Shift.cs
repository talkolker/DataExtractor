using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.DataModel.Utils;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public class Shift : sObject
    {
        [JsonIgnore]
        public ServiceResource ServiceResource { get; set; }
        [JsonProperty]
        private String ServiceResourceId { get; set; }
        [JsonIgnore]
        public ServiceTerritory ServiceTerritory { get; set; }
        [JsonProperty]
        private String ServiceTerritoryId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public bool IsOverlapsHorizon(DateTime i_HorizonStart, DateTime? i_HorizonEnd = null)
        {
            if (!i_HorizonEnd.HasValue)
            {
                i_HorizonEnd = i_HorizonStart;
            }
            bool isOverlapping = i_HorizonEnd >= StartTime && i_HorizonStart <= EndTime;

            return isOverlapping;
        }

        public virtual bool IsOverlapsHorizonPartly(DateTime i_HorizonStart, DateTime? i_HorizonEnd = null)
        {
            DateTime horizonStart = TimeZoneInfo.ConvertTimeToUtc(i_HorizonStart);
            DateTime horizonEnd;
            if (!i_HorizonEnd.HasValue)
                horizonEnd = i_HorizonStart;
            else
                horizonEnd = TimeZoneInfo.ConvertTimeToUtc(i_HorizonEnd.Value);

            bool isOverlapping = IsOverlapsHorizon(i_HorizonStart, i_HorizonEnd);
            if (!isOverlapping)
            {
                isOverlapping = horizonStart <= EndTime && horizonEnd >= EndTime;
            }
            return isOverlapping;
        }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary("ServiceResource");
            ServiceResource = DeserializationUtils.GetSingleObjectReference<ServiceResource>(ServiceResourceId, objectJson, i_ReferenceResolver);

            objectJson = removeObjectJTokenFromAdditionalDictionary("ServiceTerritory");
            ServiceTerritory = DeserializationUtils.GetSingleObjectReference<ServiceTerritory>(ServiceTerritoryId, objectJson, i_ReferenceResolver);

            if (ServiceResource != null)
            {
                ServiceResource.Shifts.Add(this);
            }
            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}

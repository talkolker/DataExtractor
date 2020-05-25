using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Standard;
using System;
using System.Collections.Generic;
using System.Text;

namespace SalesforceLibrary.DataModel.Abstraction
{
    public class ResourceTimePhasedSObject : sObject
    {
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [JsonProperty]
        protected string ServiceResourceId { get; set; }

        [JsonIgnore]
        public ServiceResource ServiceResource { get; set; }

        public ResourceTimePhasedSObject() { }

        public ResourceTimePhasedSObject(string i_ObjectId) : base(i_ObjectId) { }

        public virtual bool IsOverlapsHorizonPartly(DateTime i_HorizonStart, DateTime? i_HorizonEnd = null)
        {
            DateTime horizonStart = TimeZoneInfo.ConvertTimeToUtc(i_HorizonStart);
            DateTime horizonEnd;
            if (!i_HorizonEnd.HasValue)
                horizonEnd = i_HorizonStart;
            else
                horizonEnd = TimeZoneInfo.ConvertTimeToUtc(i_HorizonEnd.Value);

            bool isOverlapping = horizonEnd >= EffectiveStartDate && horizonStart <= EffectiveStartDate;
            if (EffectiveEndDate.HasValue && !isOverlapping)
            {
                isOverlapping = horizonStart <= EffectiveEndDate.Value && horizonEnd >= EffectiveEndDate.Value;
            }
            return isOverlapping;
        }
        
        public bool IsOverlapsHorizon(DateTime i_HorizonStart, DateTime? i_HorizonEnd = null)
        {
            if (!i_HorizonEnd.HasValue)
            {
                i_HorizonEnd = i_HorizonStart;
            }

            bool isOverlapping = i_HorizonEnd >= EffectiveStartDate;
            if (EffectiveEndDate.HasValue)
            {
                isOverlapping &= i_HorizonStart <= EffectiveEndDate.Value;
            }
            return isOverlapping;
        }
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using System;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class TimeSlot : sObject
    {
        public DayOfWeek DayOfWeek { get; set; }

        [JsonIgnore]
        public TimeSpan EndTime { get; set; }

        [JsonProperty]
        private string OperatingHoursId { get; set; }

        [JsonIgnore]
        public OperatingHours OperatingHours { get; set; }

        [JsonIgnore]
        public TimeSpan StartTime { get; set; }

        public string Type { get; set; }

        [JsonConstructor]
        private TimeSlot(string Id, string StartTime, string EndTime): base(Id)
        {
            if (!string.IsNullOrEmpty(StartTime))
                this.StartTime = TimeSpan.Parse(StartTime.Replace("Z", string.Empty));

            if (!string.IsNullOrEmpty(EndTime))
                this.EndTime = TimeSpan.Parse(EndTime.Replace("Z", string.Empty));
        }

        public TimeSlot(string i_ObjectId) : base(i_ObjectId)
        {
        }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary("OperatingHours");
            OperatingHours = DeserializationUtils.GetSingleObjectReference<OperatingHours>(OperatingHoursId, objectJson, i_ReferenceResolver);
            if (OperatingHours != null)
            {
                OperatingHours.TimeSlots.Add(this);
            }

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }
    }
}
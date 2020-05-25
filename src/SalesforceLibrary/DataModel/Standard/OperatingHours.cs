using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using System;
using System.Collections.Generic;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class OperatingHours : sObject
    {
        public TimeZoneInfo TimeZone { get; set; }

        public List<TimeSlot> TimeSlots { get; }

        [JsonConstructor]
        private OperatingHours(string Id, string TimeZone) :
            this(Id)
        {
            this.TimeZone = DeserializationUtils.GetTimeZoneFromSFString(TimeZone);
        }

        public OperatingHours(string i_ObjectId) :
            base(i_ObjectId)
        {
            TimeSlots = new List<TimeSlot>();
        }


    }
}
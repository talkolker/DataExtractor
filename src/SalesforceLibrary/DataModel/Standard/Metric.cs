using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Abstraction;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public class Metric : sObject
    {
        [JsonProperty]
        public string Message { get; set; }
        [JsonProperty]
        public double Duration { get; set; }
        [JsonProperty]
        public string Details { get; set; }
        [JsonProperty]
        public int Count { get; set; }

        public Metric() { }

        public Metric(string i_message, double i_duration)
        {
            this.Message = i_message;
            this.Duration = i_duration;
            this.Id = i_message;
        }

        public Metric(string i_message, string i_details)
        {
            this.Message = i_message;
            this.Details = i_details;
            this.Id = i_message;
        }

        public Metric(string i_message, int i_count)
        {
            this.Message = i_message;
            this.Count = i_count;
            this.Id = i_message;
        }
    }
}

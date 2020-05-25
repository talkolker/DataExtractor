using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SalesforceLibrary.DataModel
{
    public class DynamicGanttResults
    {
        private readonly Dictionary<string, ServiceAppointmentDetails> m_Changeset;

        public DynamicGanttResults()
        {
            m_Changeset = new Dictionary<string, ServiceAppointmentDetails>();
        }

        [JsonObject]
        private class ServiceAppointmentDetails
        {
            public string ServiceId { get; }
            public ServiceAppointmentInnerDetails Before { get; set; }
            public ServiceAppointmentInnerDetails After { get; set; }

            public ServiceAppointmentDetails(string i_ServiceId)
            {
                if (string.IsNullOrEmpty(i_ServiceId))
                {
                    throw new ArgumentNullException("i_ServiceId");
                }

                ServiceId = i_ServiceId;
                Before = null;
                After = null;
            }
        }

        [JsonObject]
        private class ServiceAppointmentInnerDetails
        {
            public string ResourceId { get; }
            public DateTime? ServiceStart { get; }

            public ServiceAppointmentInnerDetails(string i_ResourceId, DateTime? i_Start)
            {
                if (string.IsNullOrEmpty(i_ResourceId))
                {
                    throw new ArgumentNullException("i_ResourceId");
                }

                ResourceId = i_ResourceId;
                ServiceStart = i_Start;
            }

            public ServiceAppointmentInnerDetails(Tuple<string, DateTime?> i_Details) : this(i_Details.Item1, i_Details.Item2)
            { }
        }

        public void Add(string i_ServiceId, Tuple<string, DateTime?> i_Before, Tuple<string, DateTime?> i_After)
        {
            ServiceAppointmentDetails serviceDetails;

            if (m_Changeset.ContainsKey(i_ServiceId))
            {
                serviceDetails = m_Changeset[i_ServiceId];
            }
            else
            {
                serviceDetails = new ServiceAppointmentDetails(i_ServiceId);
                m_Changeset.Add(i_ServiceId, serviceDetails);
            }

            if (i_Before != null && !string.IsNullOrEmpty(i_Before.Item1))
            {
                serviceDetails.Before = new ServiceAppointmentInnerDetails(i_Before);
            }

            if (i_After != null && !string.IsNullOrEmpty(i_After.Item1))
            {
                serviceDetails.After = new ServiceAppointmentInnerDetails(i_After);
            }
        }

        public override string ToString()
        {
            string toStringResult = string.Empty;
            if (m_Changeset.Count > 0)
            {
                toStringResult = JsonConvert.SerializeObject(m_Changeset, Formatting.None);
            }
            return toStringResult;
        }
    }
}

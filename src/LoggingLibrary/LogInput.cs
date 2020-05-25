using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
namespace LoggingLibrary
{
    public class LogInput
    {
        private readonly string m_OrgId;
        private readonly string m_OrgType;
        private readonly string m_InstanceName;
        private readonly string m_Environment;
        private readonly string m_ServerName;
        private readonly DateTime m_TimeCreated;
        private readonly string m_Operation;
        private readonly string m_Product;
        private readonly string m_Feature;
        internal string Context { get; set; }
        internal string Message { get; set; }
        internal double Duration { get; set; }
        internal string Details { get; set; }
        internal int Count { get; set; }
        internal LogLevel Level { get; set; }

        internal LogInput(string i_OrgId, string i_OrgType, string i_InstanceName, string i_Environment, string i_Operation, string i_Product, string i_Feature)
        {
            checkForEmptyValues(i_OrgId, i_OrgType, i_InstanceName, i_Environment, i_Operation, i_Product, i_Feature);
            m_OrgId = i_OrgId;
            m_OrgType = i_OrgType;
            m_InstanceName = i_InstanceName;
            m_Environment = i_Environment;
            m_Operation = i_Operation;
            m_Product = i_Product;
            m_Feature = i_Feature;

            m_ServerName = Environment.MachineName;
            m_TimeCreated = DateTime.Now;
        }

        private void checkForEmptyValues(string i_OrgId, string i_OrgType, string i_InstanceName, string i_Environment, string i_Operation, string i_Product, string i_Feature)
        {
            if (string.IsNullOrWhiteSpace(i_OrgId))
            {
                throw new ArgumentNullException(i_OrgId);
            }
            if (string.IsNullOrWhiteSpace(i_OrgType))
            {
                throw new ArgumentNullException(i_OrgType);
            }
            if (string.IsNullOrWhiteSpace(i_InstanceName))
            {
                throw new ArgumentNullException(i_InstanceName);
            }
            if (string.IsNullOrWhiteSpace(i_Environment))
            {
                throw new ArgumentNullException(i_Environment);
            }
            if (string.IsNullOrWhiteSpace(i_Operation))
            {
                throw new ArgumentNullException(i_Operation);
            }
            if (string.IsNullOrWhiteSpace(i_Product))
            {
                throw new ArgumentNullException(i_Product);
            }
            if (string.IsNullOrWhiteSpace(i_Feature))
            {
                throw new ArgumentNullException(i_Feature);
            }
        }

        public object Clone()
        {
            return new LogInput(m_OrgId, m_OrgType, m_InstanceName, m_Environment, m_Operation, m_Product, m_Feature);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter swr = new StringWriter(sb))
            {
                using (JsonWriter jtw = new JsonTextWriter(swr))
                {
                    jtw.WriteStartObject();

                    serializableLogInput(jtw, "Customer", m_OrgId);
                    serializableLogInput(jtw, "HostName", m_OrgType);
                    serializableLogInput(jtw, "TenantName", m_InstanceName);
                    serializableLogInput(jtw, "PodName", m_Environment);
                    serializableLogInput(jtw, "Region", m_ServerName);
                    serializableLogInput(jtw, "TimeCreated", m_TimeCreated);
                    serializableLogInput(jtw, "Operation", m_Operation);
                    serializableLogInput(jtw, "Product", m_Product);
                    serializableLogInput(jtw, "Feature", m_Feature);
                    serializableLogInput(jtw, "Context", Context);
                    serializableLogInput(jtw, "Message", Message);
                    serializableLogInput(jtw, "MessageDetails", Details);

                    serializableLogInput(jtw, "Severity", Level.ToString());
                    serializableLogInput(jtw, "Level", Level.ToString());

                    jtw.WritePropertyName("Count");
                    jtw.WriteValue(Count);
                    jtw.WritePropertyName("Duration");
                    jtw.WriteValue(Duration);

                    jtw.WriteEndObject();
                }
            }
            return sb.ToString();
        }

        private static void serializableLogInput(JsonWriter i_Jtw, string i_PropertyName, string i_Data)
        {
            if (!string.IsNullOrEmpty(i_Data))
            {
                i_Jtw.WritePropertyName(i_PropertyName);
                i_Jtw.WriteValue(i_Data);
            }
        }

        private static void serializableLogInput(JsonWriter i_Jtw, string i_PropertyName, DateTime i_Data)
        {
            i_Jtw.WritePropertyName(i_PropertyName);
            i_Jtw.WriteValue(i_Data);
        }
    }
}

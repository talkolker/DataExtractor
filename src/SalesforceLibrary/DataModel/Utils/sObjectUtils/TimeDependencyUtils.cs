using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Processor;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.Requests;

namespace SalesforceLibrary.DataModel.Utils.sObjectUtils
{
    public class TimeDependencyUtils : sObjectUtils
    {
        private List<Time_Dependency__c> m_TimeDependencies;
        private HashSet<string> m_Roots;
        private class DeserializedQueryResult
        {
            public Double? totalSize;
            public Boolean? done;
            public List<Time_Dependency__c> records;
        }
        
        public override void Deserialize(string i_QueryResult, AppointmentBookingData i_ABData,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default, bool async = false)
        {
            DeserializedQueryResult deserializedQuery =
                JsonConvert.DeserializeObject<DeserializedQueryResult>(i_QueryResult);

            if (m_TimeDependencies == null)
            {
                m_TimeDependencies = new List<Time_Dependency__c>(deserializedQuery.records);
                m_Roots = new HashSet<string>();
                foreach (Time_Dependency__c dependency in m_TimeDependencies)
                {
                    m_Roots.Add(dependency.Root_Service_Appointment__c);
                }
            }
            else
            {
                if(i_ABData.TimeDependeciesByRootId == null)
                    i_ABData.TimeDependeciesByRootId = new Dictionary<string, List<Time_Dependency__c>>();

                foreach (Time_Dependency__c dependency in deserializedQuery.records)
                {
                    if(!m_TimeDependencies.Contains(dependency))
                        m_TimeDependencies.Add(dependency);
                }
                
                foreach (IGrouping<string, Time_Dependency__c> timeDependencies in m_TimeDependencies.GroupBy(dependency => dependency.Root_Service_Appointment__c))
                {
                    if(!i_ABData.TimeDependeciesByRootId.ContainsKey(timeDependencies.Key))
                        i_ABData.TimeDependeciesByRootId[timeDependencies.Key] = new List<Time_Dependency__c>();

                    foreach (Time_Dependency__c dependency in timeDependencies)
                    {
                        i_ABData.TimeDependeciesByRootId[timeDependencies.Key].Add(dependency);
                    }
                }
            }
        }

        public override string getQuery(AppointmentBookingRequest i_Request = null,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default)
        {
            string query;
            string serviceIdStr = formatIdList(new List<string>(){i_Request.ServiceID});

            if (m_TimeDependencies == null)
            {
                 query = "SELECT Id, Root_Service_Appointment__c, Dependency__c, Same_Resource__c, Service_Appointment_1__c, " +
                         "Service_Appointment_2__c FROM Time_Dependency__c " +
                         $"WHERE Service_Appointment_1__c = '{i_Request.ServiceID}' OR Service_Appointment_2__c = '{i_Request.ServiceID}'";
            }
            else if(m_TimeDependencies.Any())
            {
                query =
                    "SELECT Id, Root_Service_Appointment__c, Dependency__c, Same_Resource__c, Service_Appointment_1__c, " +
                    $"Service_Appointment_2__c FROM Time_Dependency__c WHERE Root_Service_Appointment__c IN ({serviceIdStr})";
            }
            else
            {
                return null;
            }

            return formatQueryString(query);
        }
    }
}
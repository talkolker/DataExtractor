using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.DataModel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using SalesforceLibrary.Requests;

namespace SalesforceLibrary.DataModel
{
    [JsonObject]
    public class SFOptimizationObjects
    {
        [JsonIgnore]
        private SFRefereceResolver m_refereceResolver = new SFRefereceResolver();

        public List<ServiceResource> Resources { get; set; }

        public List<ServiceAppointment> Services { get; set; }

        [JsonIgnore]
        public IDictionary<string, ServiceTerritory> Territories { get; set; }

        [JsonProperty("Territories")]
        private List<ServiceTerritory> territoriesList { get; set; }

        [JsonProperty("ServicesParents")]
        private List<JObject> parents { get; set; }

        [JsonIgnore]
        public IDictionary<string, IServiceAppoinmentParent> ServicesParents { get; set; }

        public List<TimeSlot> CalendarDays { get; set; }

        public List<ResourceCapacity> Capacities { get; set; }

        public List<ResourceAbsence> Breaks { get; set; }

        public List<ResourceAbsence> NonAvailabilities { get; set; }

        [JsonProperty("Relocations")]
        private List<ServiceTerritoryMember> relocationsList { get; set; }

        [JsonIgnore]
        public ILookup<string, ServiceTerritoryMember> Relocations { get; set; }

        public List<ServiceTerritory> SecondaryTerritories { get; set; }

        [JsonProperty("GeocodeSettings")]
        private List<Geocode_Settings__c> GeocodeSettingsList { get; set; }

        [JsonIgnore]
        public Geocode_Settings__c GeocodeSettings { get { return GeocodeSettingsList?.FirstOrDefault(); } }

        [JsonProperty("LogicSettings")]
        private List<Logic_Settings__c> logicSettingsList { get; set; }

        [JsonIgnore]
        public Logic_Settings__c LogicSettings { get { return logicSettingsList?.FirstOrDefault(); } }

        [JsonProperty("OptimizationSettings")]
        private List<OptimizationSettings__c> optimizationSettingsList { get; set; }

        [JsonIgnore]
        public OptimizationSettings__c OptimizationSettings { get { return optimizationSettingsList?.FirstOrDefault(); } }

        [JsonProperty("SupportHelper")]
        private List<SupportHelper> supportHelperList { get; set; }

        [JsonIgnore]
        public SupportHelper SupportHelper { get { return supportHelperList?.FirstOrDefault(); } }

        [JsonProperty("SchedulingPolicy")]
        public List<Scheduling_Policy__c> schedulingPolicies { get; set; }

        [JsonIgnore]
        public Scheduling_Policy__c SchedulingPolicy { get { return schedulingPolicies?.FirstOrDefault(); } }

        public List<Scheduling_Policy_Goal__c> Objectives { get; set; }

        public List<Work_Rule__c> WorkRules { get; set; }
        
        [JsonIgnore]
        public ILookup<string, Work_Rule__c> RecordTypeToWorkRule { get; set; }

        [JsonProperty("TimeDependencies")]
        private List<Time_Dependency__c> timeDependenciesList { get; set; }

        [JsonIgnore]
        public ILookup<string, Time_Dependency__c> TimeDependencies { get; set; }

        public List<Metric> Metrics { get; set; }

        public void ParseResultsAndUpdateState(string i_StringToPrase)
        {
            SFOptimizationResults optimizationResults = SFOptimizationResults.Parse(i_StringToPrase, m_refereceResolver);
            updateListsInPlace(Breaks, optimizationResults.BreaksToDelete, optimizationResults.BreaksToCreate);
        }

        internal void postDeserialization()
        {
            DeserializationUtils.AddListToReferenceResolver(territoriesList, m_refereceResolver);
            DeserializationUtils.AddListToReferenceResolver(SecondaryTerritories, m_refereceResolver);
            
            DeserializationUtils.AddListToReferenceResolver(Resources, m_refereceResolver);
            DeserializationUtils.AddListToReferenceResolver(Capacities, m_refereceResolver);
            DeserializationUtils.AddListToReferenceResolver(Breaks, m_refereceResolver);
            DeserializationUtils.AddListToReferenceResolver(NonAvailabilities, m_refereceResolver);
            DeserializationUtils.AddListToReferenceResolver(relocationsList, m_refereceResolver);
            populateServiceParentsList(m_refereceResolver);
            DeserializationUtils.AddListToReferenceResolver(Services, m_refereceResolver);

            DeserializationUtils.AddListToReferenceResolver(CalendarDays, m_refereceResolver);
            
            DeserializationUtils.AddListToReferenceResolver(logicSettingsList, m_refereceResolver);
            DeserializationUtils.AddListToReferenceResolver(GeocodeSettingsList, m_refereceResolver);
            DeserializationUtils.AddListToReferenceResolver(optimizationSettingsList, m_refereceResolver);
            DeserializationUtils.AddListToReferenceResolver(schedulingPolicies, m_refereceResolver);
            DeserializationUtils.AddListToReferenceResolver(Objectives, m_refereceResolver);
            DeserializationUtils.AddListToReferenceResolver(WorkRules, m_refereceResolver);
            DeserializationUtils.AddListToReferenceResolver(timeDependenciesList, m_refereceResolver);
            DeserializationUtils.AddListToReferenceResolver(Metrics, m_refereceResolver);
            m_refereceResolver.UpdateStoredObjectsReferences();
        }

        private void populateServiceParentsList(SFRefereceResolver i_RefereceResolver)
        {
            if (ServicesParents == null)
                ServicesParents = new Dictionary<string, IServiceAppoinmentParent>();

            if (parents != null)
            {
                JsonSerializer serializer = JsonSerializer.Create(DeserializationUtils.SFJsonSerializerSettings);

                foreach (JObject serviceParent in parents)
                {
                    JToken attributeType = serviceParent.SelectToken("attributes.type");
                    if (attributeType != null)
                    {
                        string objectType = attributeType.ToString();
                        switch (objectType)
                        {
                            case "WorkOrder":
                                WorkOrder workOrder = serviceParent.ToObject<WorkOrder>(serializer);
                                i_RefereceResolver.AddReference(workOrder);
                                ServicesParents.Add(workOrder.Id, workOrder);
                                break;
                            case "WorkOrderLineItem":
                                WorkOrderLineItem workOrderLineItem = serviceParent.ToObject<WorkOrderLineItem>(serializer);
                                i_RefereceResolver.AddReference(workOrderLineItem);
                                ServicesParents.Add(workOrderLineItem.Id, workOrderLineItem);
                                break;
                        }
                    }
                }
                parents.Clear();
                parents = null;
            }
        }

        private void createEmptyListsForNullProperties()
        {
            if (Resources == null)
                Resources = new List<ServiceResource>();

            if (Services == null)
                Services = new List<ServiceAppointment>();

            if (territoriesList == null)
                territoriesList = new List<ServiceTerritory>();
            Territories = territoriesList.ToDictionary(territory => territory.Id);

            if (ServicesParents == null)
                ServicesParents = new Dictionary<string, IServiceAppoinmentParent>();

            if (CalendarDays == null)
                CalendarDays = new List<TimeSlot>();

            if (Capacities == null)
                Capacities = new List<ResourceCapacity>();

            if (Breaks == null)
                Breaks = new List<ResourceAbsence>();

            if (NonAvailabilities == null)
                NonAvailabilities = new List<ResourceAbsence>();

            if (relocationsList == null)
                relocationsList = new List<ServiceTerritoryMember>();
            Relocations = relocationsList.ToLookup(territoryMember => territoryMember.ServiceResource?.Id);

            if (SecondaryTerritories == null)
                SecondaryTerritories = new List<ServiceTerritory>();

            if (Metrics == null)
                Metrics = new List<Metric>();

            if (timeDependenciesList == null)
                timeDependenciesList = new List<Time_Dependency__c>();
            TimeDependencies = timeDependenciesList.ToLookup(timeDependency => timeDependency.Root_Service_Appointment__r?.Id);
            
            if(WorkRules == null)
                WorkRules = new List<Work_Rule__c>();    
            RecordTypeToWorkRule = WorkRules.ToLookup(rule => rule.RecordType.Name);
        }

        public static SFOptimizationObjects Parse(string i_StringToPrase, bool i_IsManaged = true)
        {
            if (string.IsNullOrWhiteSpace(i_StringToPrase))
                throw new ArgumentNullException("i_StringToPrase");


            if (!i_IsManaged)
            {
                DeserializationUtils.NamespacesToIgnore.Add("FSL");
            }
            else
            {
                DeserializationUtils.NamespacesToIgnore.Remove("FSL");
            }
            DeserializationUtils.SFJsonSerializerSettings.ContractResolver = new SalesforceContractResolver();

            SFOptimizationObjects parsedOptimizationObjects = JsonConvert.DeserializeObject<SFOptimizationObjects>(i_StringToPrase,
                DeserializationUtils.SFJsonSerializerSettings);
            parsedOptimizationObjects.postDeserialization();
            parsedOptimizationObjects.createEmptyListsForNullProperties();
            return parsedOptimizationObjects;
        }

        private static void updateListsInPlace<T>(List<T> i_ListToUpdate, IEnumerable<T> i_objectsToRemove, IEnumerable<T> i_ObjectToAdd) where T : sObject
        {
            if (i_ListToUpdate !=null)
            {
                if (i_objectsToRemove != null)
                {
                    foreach (T objectToRemove in i_objectsToRemove)
                    {
                        i_ListToUpdate.Remove(objectToRemove);
                    }
                }

                if (i_ObjectToAdd != null)
                {
                    foreach (T objectToAdd in i_ObjectToAdd)
                    {
                        i_ListToUpdate.Add(objectToAdd);
                    }
                }
            }
        }
    }
}

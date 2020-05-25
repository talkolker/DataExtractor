using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class ServiceResource : sObject
    {
        public bool? IsActive
        {
            get { return BooleanFields["IsActive"]; }
            set { BooleanFields["IsActive"] = value; }
        }

        public bool? IsCapacityBased
        {
            get { return BooleanFields["IsCapacityBased"]; }
            set { BooleanFields["IsCapacityBased"] = value; }
        }

        public string Description { get; set; }

        public bool? IsOptimizationCapable
        {
            get { return BooleanFields["IsOptimizationCapable"]; }
            set { BooleanFields["IsOptimizationCapable"] = value; }
        }

        public string ResourceType { get; set; }

        [JsonProperty("RelatedRecordId")]
        public string UserId { get; set; }

        [JsonProperty]
        private string ServiceCrewId { get; set; }

        [JsonIgnore]
        public ServiceCrew ServiceCrew { get; set; }

        [JsonProperty("ServiceTerritories")]
        private RelatedObjectCollection<ServiceTerritoryMember> serviceTerritoriesCollection { get; set; }

        [JsonIgnore]
        public List<ServiceTerritoryMember> ServiceTerritories { get; }

        [JsonIgnore]
        public ILookup<string, ServiceTerritoryMember> ServiceTerritoriesByType { get; private set; }

        [JsonProperty("ServiceResourceCapacities")]
        private RelatedObjectCollection<ResourceCapacity> serviceResourceCapacitiesCollection { get; set; }

        [JsonIgnore]
        public List<ResourceCapacity> Capacities { get; }

        [JsonProperty("ServiceResourceSkills")]
        private RelatedObjectCollection<ServiceResourceSkill> serviceResourceSkillsCollection { get; set; }

        [JsonProperty("ShiftServiceResources")]
        private RelatedObjectCollection<Shift> resourceShiftsCollection { get; set; }

        [JsonIgnore]
        public List<ServiceResourceSkill> Skills { get; }

        [JsonIgnore]
        public List<Shift> Shifts { get; }

        [JsonProperty("ServiceCrewMembers")]
        private RelatedObjectCollection<ServiceCrewMember> serviceCrewMembersCollection { get; set; }

        [JsonIgnore]
        public List<ServiceCrewMember> ServiceCrewMembers { get; }

        public ServiceResource() :
            this(null)
        { }

        public ServiceResource(string i_ObjectId) : base(i_ObjectId)
        {
            ServiceTerritories = new List<ServiceTerritoryMember>();
            Capacities = new List<ResourceCapacity>();
            Skills = new List<ServiceResourceSkill>();
            ServiceCrewMembers = new List<ServiceCrewMember>();
            Shifts = new List<Shift>();
        }

        internal override void updateReferencesAfterDeserialize(SFRefereceResolver i_ReferenceResolver, bool i_ShouldAddToRelatedLists = true)
        {
            JToken objectJson = removeObjectJTokenFromAdditionalDictionary("ServiceCrew");
            ServiceCrew = DeserializationUtils.GetSingleObjectReference<ServiceCrew>(ServiceCrewId, objectJson, i_ReferenceResolver);
            if (ServiceCrew != null)
            {
                ServiceCrew.ServiceResources.Add(this);
            }

            DeserializationUtils.ProcessRelatedObjectsCollection(serviceTerritoriesCollection, i_ReferenceResolver);
            ServiceTerritoriesByType = ServiceTerritories.ToLookup(stm => stm.TerritoryType);
            serviceTerritoriesCollection = null;

            DeserializationUtils.ProcessRelatedObjectsCollection(serviceResourceCapacitiesCollection, i_ReferenceResolver);
            serviceResourceCapacitiesCollection = null;

            DeserializationUtils.ProcessRelatedObjectsCollection(serviceResourceSkillsCollection, i_ReferenceResolver);
            serviceResourceSkillsCollection = null;

            DeserializationUtils.ProcessRelatedObjectsCollection(serviceCrewMembersCollection, i_ReferenceResolver);
            serviceCrewMembersCollection = null;

            DeserializationUtils.ProcessRelatedObjectsCollection(resourceShiftsCollection, i_ReferenceResolver);
            resourceShiftsCollection = null;

            base.updateReferencesAfterDeserialize(i_ReferenceResolver);
        }

        public ServiceTerritoryMember GetServiceTerritoryMemberByDateTime(DateTime i_SearchTime)
        {
            return GetServiceTerritoryMembersWhichOverlapHorizon(i_SearchTime).FirstOrDefault();
        }

        public ServiceTerritoryMember GetServiceTerritoryMemberByDateTime(DateTime i_SearchTimeStart, DateTime i_SearchTimeFinish)
        {
            return GetServiceTerritoryMembersWhichOverlapHorizon(i_SearchTimeStart, i_SearchTimeFinish).FirstOrDefault();
        }

        public IEnumerable<ServiceTerritoryMember> GetServiceTerritoryMembersWhichOverlapHorizon(DateTime i_HorizonStart, DateTime? i_HorizonEnd = null)
        {
            return ServiceTerritories.Where(stm => (stm.TerritoryType != "S" && stm.IsOverlapsHorizon(i_HorizonStart, i_HorizonEnd))).OrderByDescending(stm => stm.TerritoryType);
        }
    }
}
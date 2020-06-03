using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SalesforceLibrary.DataModel;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.DataModel.Utils;
using SalesforceLibrary.DataModel.Utils.sObjectUtils;
using SalesforceLibrary.Requests;

namespace Processor
{
    public class AppointmentBookingData
    {
        private string m_PolicyId;
        private Dictionary<string, List<Work_Rule__c>> m_RulesByDevName;
        private Dictionary<string, List<Service_Goal__c>> m_ObjectivesByDevName;
        private ServiceAppointment m_ServiceToSchedule;
        private Dictionary<string, ServiceAppointment> m_ServicesById;
        private Dictionary<string, List<Time_Dependency__c>> m_TimeDependeciesByRootId;
        private List<ServiceTerritoryMember> m_TerritoryMembers;
        private Dictionary<string, ServiceParent> m_ServiceParent;
        private Logic_Settings__c m_LogicSettings;
        private List<OperatingHours> m_VisitingHours;
        private HashSet<string> m_STMResourcesIDs;
        private Dictionary<string, ServiceResource> m_CandidatesById;
        public HashSet<string> m_UnLicensedUsers;
        public AdditionalObjects m_ABAdditionalObjects;

        public class AdditionalObjects
        {
            public Dictionary<string, sObject> ServicesById { get; set; }
            public Dictionary<string, sObject> ResBreaksAndShiftsByResId { get; set; }
            public List<sObject> ResourceTerritories { get; set; }
            public  List<sObject> Capacities { get; set; }
            public List<sObject> Calendars { get; set; }
        }
        
        public AdditionalObjects ABAdditionalObjects
        {
            get => m_ABAdditionalObjects;
            set => m_ABAdditionalObjects = value;
        }
        
        public HashSet<string> UnLicensedUsers
        {
            get => m_UnLicensedUsers;
            set => m_UnLicensedUsers = value;
        }

        public Dictionary<string, ServiceResource> CandidatesById
        {
            get => m_CandidatesById;
            set => m_CandidatesById = value;
        }

        public HashSet<string> STMResourcesIDs
        {
            get => m_STMResourcesIDs;
            set => m_STMResourcesIDs = value;
        }

        public List<OperatingHours> VisitingHours
        {
            get => m_VisitingHours;
            set => m_VisitingHours = value;
        }

        public Logic_Settings__c LogicSettings
        {
            get => m_LogicSettings;
            set => m_LogicSettings = value;
        }

        public Dictionary<string, ServiceParent> ServiceParent
        {
            get => m_ServiceParent;
            set => m_ServiceParent = value;
        }

        public List<ServiceTerritoryMember> TerritoryMembers
        {
            get => m_TerritoryMembers;
            set => m_TerritoryMembers = value;
        }


        public ServiceAppointment ServiceToSchedule
        {
            get => m_ServiceToSchedule;
            set => m_ServiceToSchedule = value;
        }

        public Dictionary<string, List<Time_Dependency__c>> TimeDependeciesByRootId
        {
            get => m_TimeDependeciesByRootId;
            set => m_TimeDependeciesByRootId = value;
        }

        public Dictionary<string, ServiceAppointment> ServicesById
        {
            get => m_ServicesById;
            set => m_ServicesById = value;
        }

        public string PolicyId
        {
            get => m_PolicyId;
            set => m_PolicyId = value;
        }

        public Dictionary<string, List<Work_Rule__c>> RulesByDevName
        {
            get => m_RulesByDevName;
            set => m_RulesByDevName = value;
        }

        public Dictionary<string, List<Service_Goal__c>> ObjectivesByDevName
        {
            get => m_ObjectivesByDevName;
            set => m_ObjectivesByDevName = value;
        }

        public AppointmentBookingData(AppointmentBookingRequest i_Request)
        {
            m_PolicyId = i_Request.PolicyId;
            m_RulesByDevName = new Dictionary<string, List<Work_Rule__c>>();
            m_ObjectivesByDevName = new Dictionary<string, List<Service_Goal__c>>();
            m_ABAdditionalObjects = new AdditionalObjects();
        }
    }
}
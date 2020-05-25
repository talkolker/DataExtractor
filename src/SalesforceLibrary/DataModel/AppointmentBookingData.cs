using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SalesforceLibrary.DataModel;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.DataModel.Utils;
using SalesforceLibrary.Requests;

namespace Processor
{
    public class AppointmentBookingData
    {
        private string m_PolicyId;
        private Dictionary<string, List<Work_Rule__c>> m_Rules;
        private List<Scheduling_Policy_Goal__c> m_Objectives;
        private Dictionary<string, ServiceAppointment> m_Services;
        public Logic_Settings__c m_LogicSettings;

        public Logic_Settings__c LogicSettings
        {
            get => m_LogicSettings;
            set => m_LogicSettings = value;
        }

        public Dictionary<string, ServiceAppointment> Services
        {
            get => m_Services;
            set => m_Services = value;
        }

        public string PolicyId
        {
            get => m_PolicyId;
            set => m_PolicyId = value;
        }

        public Dictionary<string, List<Work_Rule__c>> Rules
        {
            get => m_Rules;
            set => m_Rules = value;
        }

        public List<Scheduling_Policy_Goal__c> Objectives
        {
            get => m_Objectives;
            set => m_Objectives = value;
        }

        public AppointmentBookingData(AppointmentBookingRequest i_Request)
        {
            m_PolicyId = i_Request.PolicyId;
            m_Rules = new Dictionary<string, List<Work_Rule__c>>();
            m_Objectives = new List<Scheduling_Policy_Goal__c>();
            m_LogicSettings = new Logic_Settings__c();
            m_LogicSettings.MDT_Boolean_Field__c = i_Request.MDTBooleanField;
        }
    }
}
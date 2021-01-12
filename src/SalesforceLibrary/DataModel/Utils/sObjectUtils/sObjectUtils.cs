using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Processor;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.DataModel.Standard;
using SalesforceLibrary.Requests;

namespace SalesforceLibrary.DataModel.Utils.sObjectUtils
{
    public abstract class sObjectUtils
    {
        //TODO: add try catch around every implementation of this method
        public abstract void Deserialize(string i_QueryResult, AppointmentBookingData i_ABData,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default, bool async = false);
        public abstract string getQuery(AppointmentBookingRequest i_Request = null,
            AdditionalObjectsUtils.eAdditionalObjectQuery i_AdditionalObjQuery = default);

        protected string formatQueryString(string i_Query)
        {
            string formattedString = Regex.Replace(i_Query, "(\\s+,|,\\s+|\\s+,\\s+|,)", " , ");
            formattedString = Regex.Replace(formattedString, "(\\s+=|=\\s+|\\s+=\\s+|=)", " = ");
            formattedString = Regex.Replace(formattedString, "(\\s+\\(|\\(\\s+|\\s+\\(\\s+|\\()", " ( ");
            formattedString = Regex.Replace(formattedString, "(\\s+\\)|\\)\\s+|\\s+\\)\\s+|\\))", " ) ");
            formattedString = Regex.Replace(formattedString, "(\\s+)", "+");
            return formattedString;
        }

        protected string formatIdList(List<string> i_IdsList)
        {
            if (i_IdsList.Count == 0)
                return "''";
            
            string formattedList = "";
            foreach (string Id in i_IdsList)
            {
                formattedList = string.Concat(formattedList, " \'" + Id + "\' ,");
            }

            formattedList = formattedList.Remove(formattedList.Length - 1);
            return formattedList;
        }
        
        protected string formatList(List<string> i_List)
        {
            if (i_List.Count == 0)
                return "''";
            
            string formattedList = "";
            foreach (string Id in i_List)
            {
                formattedList = string.Concat(formattedList, Id + ", ");
            }

            formattedList = formattedList.Remove(formattedList.Length - 2);
            return formattedList;
        }

        protected string formatDate(DateTime i_Date)
        {
            return i_Date.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
        
        protected void calculateHorizonByMaxDaysSearchSlot(ServiceAppointment i_Service, DateTime i_DateTimeNow, double i_SearchSlotsMaxDays, out DateTime start, out DateTime finish){
            DateTime? resourcesHorizonStart = i_Service.EarliestStartTime;
            DateTime? resourcesHorizonFinish = i_Service.DueDate;
            if (resourcesHorizonStart < i_DateTimeNow) {
                resourcesHorizonStart = i_DateTimeNow;
            }
            
            TimeSpan currDaysInterval = (resourcesHorizonFinish.Value.Subtract(resourcesHorizonStart.Value));

            double maxDays = i_SearchSlotsMaxDays;
            if (currDaysInterval.Days > maxDays) {
                resourcesHorizonFinish = resourcesHorizonStart.Value.AddDays(maxDays);
            }

            start = resourcesHorizonStart.Value.AddDays(-1);
            finish = resourcesHorizonFinish.Value.AddDays(1);
        }
    }
}
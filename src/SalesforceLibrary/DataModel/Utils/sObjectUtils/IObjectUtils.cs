using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Processor;
using SalesforceLibrary.DataModel.Abstraction;
using SalesforceLibrary.DataModel.FSL;
using SalesforceLibrary.Requests;

namespace SalesforceLibrary.DataModel.Utils.sObjectUtils
{
    public abstract class IObjectUtils
    {
        //TODO: add try catch around every implementation of this method
        public abstract void Deserialize(string i_QueryResult, AppointmentBookingData i_ABData);
        public abstract string getQuery(AppointmentBookingRequest i_Request = null);

        protected string formatQueryString(string i_Query)
        {
            string formattedString = Regex.Replace(i_Query, "(\\s+,|,\\s+|\\s+,\\s+|,)", " , ");
            formattedString = Regex.Replace(formattedString, "(\\s+=|=\\s+|\\s+=\\s+|=)", " = ");
            formattedString = Regex.Replace(formattedString, "(\\s+\\(|\\(\\s+|\\s+\\(\\s+|\\()", " ( ");
            formattedString = Regex.Replace(formattedString, "(\\s+\\)|\\)\\s+|\\s+\\)\\s+|\\))", " ) ");
            formattedString = Regex.Replace(formattedString, "(\\s+)", "+");
            return formattedString;
        }

        protected string formatList(List<string> i_IdsList)
        {
            if (i_IdsList.Count == 0)
                return " { } ";
                
            string formattedList = " { ' ";
            foreach (string Id in i_IdsList)
            {
                formattedList = string.Concat(formattedList, Id + " ' , ' ");
            }

            formattedList = formattedList.Remove(formattedList.Length - 7);
            formattedList += " ' } ";

            return formattedList;
        }
    }
}
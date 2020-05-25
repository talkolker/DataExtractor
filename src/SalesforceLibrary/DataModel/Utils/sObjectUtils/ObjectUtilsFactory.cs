using System;
using Processor;
using SalesforceLibrary.DataModel.FSL;

namespace SalesforceLibrary.DataModel.Utils.sObjectUtils
{
    public class ObjectUtilsFactory
    {
        public static IObjectUtils CreateUtilsByType(Type i_ObjectType, AppointmentBookingData i_ABData = null)
        {
            IObjectUtils objectUtils;
            //TODO: check the value of Name in managed package
            switch (i_ObjectType.Name)
            {
                case "Work_Rule__c":
                    objectUtils =  new WorkRuleUtils();
                    break;
                
                case "Scheduling_Policy_Goal__c":
                    objectUtils = new ObjectiveUtils();
                    break;
                
                case "ServiceAppointment":
                    objectUtils = new ServiceAppointmentUtils(i_ABData);
                    break;

                default:
                    objectUtils = null;
                    break;
            }

            return objectUtils;
        }
    }
}
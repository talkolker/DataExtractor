using System;
using Processor;
using SalesforceLibrary.DataModel.FSL;

namespace SalesforceLibrary.DataModel.Utils.sObjectUtils
{
    public class ObjectUtilsFactory
    {
        public static sObjectUtils CreateUtilsByType(Type i_ObjectType = null, AppointmentBookingData i_ABData = null, string i_UtilClassName = null)
        {
            sObjectUtils objectUtils;
            
            if (i_UtilClassName != null)
            {
                return createUtilsByClassName(i_UtilClassName, i_ABData);
            }
            
            //TODO: check the value of Name in managed package
            switch (i_ObjectType.Name)
            {
                case "Work_Rule__c":
                    objectUtils = new WorkRuleUtils();
                    break;

                case "Scheduling_Policy_Goal__c":
                    objectUtils = new ObjectiveUtils();
                    break;

                case "ServiceAppointment":
                    objectUtils = new ServiceAppointmentUtils(i_ABData);
                    break;

                case "Time_Dependency__c":
                    objectUtils = new TimeDependencyUtils();
                    break;
                
                case "ServiceTerritoryMember":
                    objectUtils = new STMUtils(i_ABData);
                    break;
                
                case "OperatingHours":
                    objectUtils = new OperatingHoursUtils(i_ABData);
                    break;
                
                case "ServiceResource":
                    objectUtils = new ServiceResourceUtils(i_ABData);
                    break;

                default:
                    objectUtils = null;
                    break;
            }

            return objectUtils;
        }

        private static sObjectUtils createUtilsByClassName(string i_UtilClassName, AppointmentBookingData i_ABData)
        {
            sObjectUtils objectUtils;
            switch (i_UtilClassName)
            {
                case "MSTServiceUtils":
                    objectUtils = new MSTServiceUtils(i_ABData);
                    break;
                
                case "WorkOrder":
                case "WorkOrderLineItem":
                case "Account":
                    objectUtils = new ServiceParentUtils(i_ABData);
                    break;

                case "UserLicense":
                    objectUtils = new LicensedUsersUtils(i_ABData);
                    break;
                    
                case "AdditionalObjects":
                    objectUtils = new AdditionalObjectsUtils(i_ABData);
                    break;
                
                default:
                    objectUtils = null;
                    break;
            }

            return objectUtils;
        }
    }
}
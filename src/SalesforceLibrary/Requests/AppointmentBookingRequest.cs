using System.Collections.Generic;
using SalesforceLibrary.DataModel.Standard;

namespace SalesforceLibrary.Requests
{
    public class AppointmentBookingRequest : SFDCScheduleRequest
    {
        public string SchedulingPolicyID;
        
        public string TravelUnit;

        public string PolicyId => SchedulingPolicyID;
        public string ServiceID { get; set; }
        public double SearchSlotsMaxDays { get; set; }
        
        public bool ApprovedAbsences { get; set; }

        public bool IsEmergency;
    }
}
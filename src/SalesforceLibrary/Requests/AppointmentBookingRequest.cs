using System.Collections.Generic;

namespace SalesforceLibrary.Requests
{
    public class AppointmentBookingRequest : SFDCScheduleRequest
    {
        public string SchedulingPolicyID;
        
        public string MDTBooleanField;

        public string PolicyId => SchedulingPolicyID;
        public List<string> ServiceIDs { get; set; }

        public bool IsEmergency;
    }
}
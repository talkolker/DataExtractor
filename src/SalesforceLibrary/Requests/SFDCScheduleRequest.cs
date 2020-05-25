using System;

namespace SalesforceLibrary.Requests
{
    public abstract class SFDCScheduleRequest
    {
        public string RefreshToken { get; set; }

        public bool IsTest { get; set; }

        public bool IsManaged { get; set; }

        public string CustomSFDCAuthURL { get; set; }

        //public bool UseEdge { get; set; }

        public DateTime Start { get; set; }

        public DateTime Finish { get; set; }

        //public string SchedulingPolicyID { get; set; }

        public SFToClickPropertyTranslation[] ServicePropertiesTranslations { get; set; }

        public SFToClickPropertyTranslation[] ResourcePropertiesTranslations { get; set; }

        //public string ServiceDurationHours { get; set; }

        //public string ServiceDurationMinutes { get; set; }

        //public string ServiceDurationDays { get; set; }

        public string OAASDataID { get; set; }

        public string BreakRecordTypeID { get; set; }

        public bool IsTravelTriggerEnabled { get; set; }

        public bool[] SupportedFeatures { get; set; }

        // used by monitor or Auth rate limit
        //public double BGOMaxRunTimeMinutes { get; set; }

        //public double SingleTaskMaxRunTimeSeconds { get; set; }

        public bool PredictiveTravelEnabled { get; set; }

        //public int NumberOfServices { get; set; }

        //public int MaximumNumberOfServicesPer24Hours { get; set; }

        public string InstanceName { get; set; }

        public string OrganizationType { get; set; }

        public DateTime QueueStartTime { get; set; }

        //public int NumberOfConcurrentRequestsAllowed { get; set; }

        // keep fields at bottom of request
        public string ErrorObjectKey { get; set; }

        public string RequestIdentifier { get; set; }

        public string OrganizationId { get; set; }

        public string AsyncIdentifier
        {
            get
            {
                return string.Format("{0}_{1}", OrganizationId, RequestIdentifier);
            }
        }
    }
}

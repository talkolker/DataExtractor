using System;

namespace SalesforceLibrary.Requests
{
    public class SFDCResourceDayOptimizationRequest : SFDCScheduleRequest
    {
        public bool AllTasksMode { get; set; }

        public string[] CalloutServiceIds { get; set; }

        public string[] Locations { get; set; }

        public string RdoResourceId { get; set; }

        public DateTime? NowAtSchedule { get; set; }

        public bool KeepSameOrder { get; set; }

        public bool ClearGantt { get; set; }

        public string TriggeringRecipeId { get; set; }

        public string StatusToUpdate { get; set; }

        public string InJeopardyReason { get; set; }
        public Double longitudeLastKnownLocation { get; set; }
        public Double latitudeLastKnownLocation { get; set; }
    }
}

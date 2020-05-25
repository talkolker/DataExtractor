namespace SalesforceLibrary.Requests
{
    public class SFDCOptimizationRequest : SFDCScheduleRequest
    {
        public bool IncludeServicesWithEmptyLocation { get; set; }

        public string[] Locations { get; set; }

        public string FilterFieldAPIName { get; set; }

        public bool AllTasksMode { get; set; }
    }
}

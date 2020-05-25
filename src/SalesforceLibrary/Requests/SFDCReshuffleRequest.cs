namespace SalesforceLibrary.Requests
{
    public class SFDCReshuffleRequest : SFDCScheduleRequest
    {
        public string[] ReshuffleServiceIds { get; set; }

        public bool IsNativeReshuffle { get; set; }
    }
}

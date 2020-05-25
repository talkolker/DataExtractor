using System;

namespace SalesforceLibrary.DataModel.Abstraction
{
    public class CustomJunctionObject : sObject
    {
        private string resourceToMatchId;
        private string matchObjectId;
        private DateTime start;
        private DateTime finish;

        public string ResourceToMatchId
        {
            get => resourceToMatchId;
            set => resourceToMatchId = value;
        }

        public string MatchObjectId
        {
            get => matchObjectId;
            set => matchObjectId = value;
        }

        public DateTime Start
        {
            get => start;
            set => start = value;
        }

        public DateTime Finish
        {
            get => finish;
            set => finish = value;
        }
    }
}
using Newtonsoft.Json;
using SalesforceLibrary.DataModel.Abstraction;
using System;
using System.Collections.Generic;

namespace SalesforceLibrary.DataModel.Standard
{
    [JsonObject]
    public partial class ServiceCrew : sObject
    {
        public int? CrewSize
        {
            get
            {
                double? tempValue = NumericFields["CapacityInHours"];
                if (tempValue.HasValue)
                {
                    return Convert.ToInt32(tempValue.Value);
                }
                return null;
            }
            set { NumericFields["CapacityInHours"] = value; }
        }

        [JsonIgnore]
        public List<ServiceResource> ServiceResources { get; }


        public List<ServiceCrewMember> ServiceCrewMembers { get; }

        public ServiceCrew() :
            this(null)
        { }

        public ServiceCrew(string i_ObjectId) : base(i_ObjectId)
        {
            ServiceResources = new List<ServiceResource>();
            ServiceCrewMembers = new List<ServiceCrewMember>();
        }
    }
}
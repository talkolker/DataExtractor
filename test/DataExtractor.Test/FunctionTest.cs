using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Processor;

namespace DataExtractor.Tests
{
  public class FunctionTest
  {
      private static readonly HttpClient client = new HttpClient();

    [Fact]
    public async Task TestRestApi()
    {
        var function = new Function();
        string measurments = await function.FunctionHandler();

        Assert.NotNull(measurments);
        Assert.NotEqual("{}", measurments);
    }
    
    
    [Fact]
    public async Task TestApexRestService()
    {
        string requestXML = "<AppointmentBookingRequest><RefreshToken>5Aep861i3pidIObecHklRnSH1FnIZsznQb_i3Jo9UC6ey5emPA8bFpnVVfFu5kexGfE0sWAb1qtfPkJLVQsT4Sd</RefreshToken><IsEmergency>false</IsEmergency><ServiceID>08pe0000000CbHtAAK</ServiceID><IsTest>true</IsTest><IsManaged>false</IsManaged><SchedulingPolicyID>a0Qe000000AzjJNEAZ</SchedulingPolicyID><InstanceName>CS15</InstanceName><OrganizationId>00De0000005T9GFEA0</OrganizationId><OrganizationType>Enterprise Edition</OrganizationType><TravelUnit>km</TravelUnit><SearchSlotsMaxDays>14</SearchSlotsMaxDays><ApprovedAbsences>true</ApprovedAbsences></AppointmentBookingRequest>";
        
        string measurments = RequestProcessor.GetDataByApexRestService(requestXML);

        Console.WriteLine("Finished successfully");
    }
  }
}
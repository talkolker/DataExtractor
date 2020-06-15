using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json;
using Processor;


using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DataExtractor
{

    public class Function
    {
        public async Task FunctionHandler()
        {
            string requestXML = "<AppointmentBookingRequest><RefreshToken>5Aep861i3pidIObecHklRnSH1FnIZsznQb_i3Jo9UC6ey5emPA8bFpnVVfFu5kexGfE0sWAb1qtfPkJLVQsT4Sd</RefreshToken><IsEmergency>false</IsEmergency><ServiceID>08pe0000000CbHtAAK</ServiceID><IsTest>true</IsTest><IsManaged>false</IsManaged><SchedulingPolicyID>a0Qe000000AzjJNEAZ</SchedulingPolicyID><InstanceName>CS15</InstanceName><OrganizationId>00De0000005T9GFEA0</OrganizationId><OrganizationType>Enterprise Edition</OrganizationType><TravelUnit>km</TravelUnit><SearchSlotsMaxDays>14</SearchSlotsMaxDays><ApprovedAbsences>true</ApprovedAbsences></AppointmentBookingRequest>";
            try
            {
                Stopwatch watchWholeProcess = new Stopwatch();
                watchWholeProcess.Start();
                RequestProcessor.ProcessRequest(requestXML);
                watchWholeProcess.Stop();

                LambdaLogger.Log("Whole process by REST API including login to SF took: " + watchWholeProcess.ElapsedMilliseconds +
                                 " ms\n");
                watchWholeProcess.Reset();
                
                RequestProcessor.GetDataByApexRestService(requestXML);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                LambdaLogger.Log(ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}

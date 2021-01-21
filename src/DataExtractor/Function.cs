using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
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
        public async Task<string> FunctionHandler()
        {
            string requestXML = "<AppointmentBookingRequest><RefreshToken>5Aep861i3pidIObecHklRnSH1FnIZsznQb_i3Jo9UC6ey5emPA8bFpnVVfFu5kexGfE0sWAb1qtfPkJLVQsT4Sd</RefreshToken><IsEmergency>false</IsEmergency><ServiceID>08pe0000000CbHtAAK</ServiceID><IsTest>true</IsTest><IsManaged>false</IsManaged><SchedulingPolicyID>a0Qe000000AzjJNEAZ</SchedulingPolicyID><InstanceName>CS15</InstanceName><OrganizationId>00De0000005T9GFEA0</OrganizationId><OrganizationType>Enterprise Edition</OrganizationType><TravelUnit>km</TravelUnit><SearchSlotsMaxDays>14</SearchSlotsMaxDays><ApprovedAbsences>true</ApprovedAbsences></AppointmentBookingRequest>";
            try
            {
                //string ping = pingOrg();

                //string emptyMeasures = RequestProcessor.SendEmptyRequest(requestXML);
                
                string restMeasurements = RequestProcessor.ProcessRequest(requestXML);

                //string apexRestMeasurments = RequestProcessor.GetDataByApexRestService(requestXML);

                //string results = emptyMeasures;
                string results = restMeasurements;
                //string results = apexRestMeasurments;
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                LambdaLogger.Log(ex.Message + "\n" + ex.StackTrace);
                throw new Exception(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private string pingOrg()
        {
            Ping pingSender = new Ping();
            string header = "\n~~~~~~~~ PING ORG ~~~~~~~~\n";
            long elapsed = 0;

            string response = header;
            for (int i = 0; i < 100; i++)
            {
                PingReply reply = pingSender.Send("cs15.salesforce.com");
                if (reply.Status == IPStatus.Success)
                {
                    //response += $"\nAddress: {reply.Address.ToString()}";
                    //response += $"\nRoundTrip time: {reply.RoundtripTime}\n";
                    elapsed += reply.RoundtripTime;
                }
            }

            return response + (elapsed / 100);
        }
        
        private long IPToLong(string ipAddress)
        {
            System.Net.IPAddress ip;
            if (System.Net.IPAddress.TryParse(ipAddress, out ip))
                return (((long)ip.GetAddressBytes()[0] << 24) | ((int)ip.GetAddressBytes()[1] << 16) | ((int)ip.GetAddressBytes()[2] << 8) | ip.GetAddressBytes()[3]);
            else
                return 0;
        }
    }
}

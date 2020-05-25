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

        private static readonly HttpClient client = new HttpClient();

        private static async Task<string> GetCallingIP()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "AWS Lambda .Net Client");

            var msg = await client.GetStringAsync("http://checkip.amazonaws.com/").ConfigureAwait(continueOnCapturedContext:false);

            return msg.Replace("\n","");
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {

            //var location = await GetCallingIP();
            string body = "finished";
            string requestXML = "<AppointmentBookingRequest><RefreshToken>5Aep861i3pidIObecHklRnSH1FnIZsznQb_i3Jo9UC6ey5emPA8bFpnVVfFu5kexGfE0sWAb1qtfPkJLVQsT4Sd</RefreshToken><IsEmergency>false</IsEmergency><ServiceIDs><string>08pe0000000CbHtAAK</string></ServiceIDs><IsTest>true</IsTest><IsManaged>false</IsManaged><UseEdge>true</UseEdge><Start>2020-05-03T00:00:00Z</Start><Finish>2020-05-04T00:00:00Z</Finish><SchedulingPolicyID>a0Qe000000B06dnEAB</SchedulingPolicyID><BreakRecordTypeID>012e000000016xAAAQ</BreakRecordTypeID><IsTravelTriggerEnabled>true</IsTravelTriggerEnabled><SingleTaskMaxRunTimeSeconds>1</SingleTaskMaxRunTimeSeconds><PredictiveTravelEnabled>false</PredictiveTravelEnabled><NumberOfServices>1</NumberOfServices><MaximumNumberOfServicesPer24Hours>50000</MaximumNumberOfServicesPer24Hours><InstanceName>CS15</InstanceName><OrganizationType>Enterprise Edition</OrganizationType><QueueStartTime>2020-04-20T08:56:00.4639112+00:00</QueueStartTime><NumberOfConcurrentRequestsAllowed>2</NumberOfConcurrentRequestsAllowed><RequestIdentifier>jtzzpsT1XkoID69</RequestIdentifier><ErrorObjectKey>jtzzpsT1XkoID69</ErrorObjectKey><OrganizationId>00De0000005T9GFEA0</OrganizationId><AllTasksMode>true</AllTasksMode><Locations><string>0Hhe00000009lyECAQ</string></Locations><RdoResourceId>0Hne0000000CaVCCA0</RdoResourceId><NowAtSchedule>2020-04-20T08:55:57.658Z</NowAtSchedule><longitudeLastKnownLocation>0</longitudeLastKnownLocation><latitudeLastKnownLocation>0</latitudeLastKnownLocation><ClearGantt>false</ClearGantt><MDTBooleanField>IsMultiDay__c</MDTBooleanField></AppointmentBookingRequest>";
            
            try
            {
                RequestProcessor.ProcessRequest(requestXML);
            }
            catch (Exception ex)
            {
                body = ex.Message;
            }

            return new APIGatewayProxyResponse
            {
                //TODO: print how long all querying took
                Body = body,
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FunctionApps
{
    public static class MonitorFunctionExample
    {  
        /// <summary>
        /// Entry point for the durable function 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="starter"></param>
        /// <param name="log"></param>
        /// <code>curl --location --request GET 'http://localhost:7071/api/HttpStart'</code>
        /// <returns></returns>
        [FunctionName("MonitorHttpStart")]
        public static async Task<HttpResponseMessage> MonitorHttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("MonitorOrchestration", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("MonitorOrchestration")]
        public static async Task MonitorOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            int jobId = context.GetInput<int>();
            int pollingInterval = GetPollingInterval();
            DateTime expiryTime = GetExpiryTime();

            while (context.CurrentUtcDateTime < expiryTime)
            {
                var jobStatus = await context.CallActivityAsync<string>("GetJobStatus-Activity", jobId);
                if (jobStatus == "Completed")
                {
                    // Perform an action when a condition is met.
                    await context.CallActivityAsync("SendAlert-Activity", Guid.NewGuid().ToString());
                    break;
                }

                // Orchestration sleeps until this time.
                var nextCheck = context.CurrentUtcDateTime.AddSeconds(pollingInterval);
                await context.CreateTimer(nextCheck, CancellationToken.None);
            }

            // Perform more work here, or let the orchestration end.
        }

        private static DateTime GetExpiryTime()
        {
            return DateTime.Now.AddSeconds(20);
        }

        private static int GetPollingInterval()
        {
            return 1;
        }

        [FunctionName("SendAlert-Activity")]
        public static void SendAlert([ActivityTrigger] string machineid, ILogger log)
        {
            log.LogInformation($"Sending alert to {machineid}");
        }

        [FunctionName("GetJobStatus-Activity")]
        public static string GetJobStatus([ActivityTrigger] string jobId, ILogger log)
        {
            return "Completed";
        }

    }
}
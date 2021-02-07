using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FunctionApps.DurableFunctions
{
    public static class HumanInteractionFunctionExample
    {
        /// <summary>
        /// Entry point for the durable function 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="starter"></param>
        /// <param name="log"></param>
        /// <code>curl --location --request GET 'http://localhost:7071/api/HumanInteractionHttpStart'</code>
        /// <returns></returns>
        [FunctionName("HumanInteractionHttpStart")]
        public static async Task<HttpResponseMessage> HumanInteractionHttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            var instanceId = await starter.StartNewAsync("ApprovalWorkflow", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("ApprovalWorkflow")]
        public static async Task Run(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            await context.CallActivityAsync("RequestApproval", null);
            using (var timeoutCts = new CancellationTokenSource())
            {
                DateTime dueTime = context.CurrentUtcDateTime.AddHours(72);
                Task durableTimeout = context.CreateTimer(dueTime, timeoutCts.Token);

                Task<bool> approvalEvent = context.WaitForExternalEvent<bool>("ApprovalEvent");
                if (approvalEvent == await Task.WhenAny(approvalEvent, durableTimeout))
                {
                    timeoutCts.Cancel();
                    await context.CallActivityAsync("ProcessApproval", approvalEvent.Result);
                }
                else
                {
                    await context.CallActivityAsync("Escalate", null);
                }
            }
        }

        [FunctionName("RequestApproval")]
        public static void RequestApproval([ActivityTrigger] object input, ILogger log)
        {
            log.LogInformation($"Request Approval ...");
        }

        [FunctionName("ApprovalEvent")]
        public static bool ApprovalEvent([ActivityTrigger] ILogger log)
        {
            log.LogInformation($"Approval Event ...");
            return true;
        }

        [FunctionName("ProcessApproval")]
        public static void ProcessApproval([ActivityTrigger] bool result, ILogger log)
        {
            log.LogInformation($"Process Approval Event ...");
        }

        [FunctionName("RaiseEventToOrchestration")]
        public static async Task RaiseEventToOrchestration(
            [HttpTrigger] string instanceId,
            [DurableClient] IDurableOrchestrationClient client)
        {
            await client.RaiseEventAsync(instanceId, "ApprovalEvent", true);
        }

    }
}
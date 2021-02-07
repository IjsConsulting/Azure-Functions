using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FunctionApps.DurableFunctions
{
    public static class DurableFunctionExample
    {  
        /// <summary>
        /// Entry point for the durable function 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="starter"></param>
        /// <param name="log"></param>
        /// <code>curl --location --request GET 'http://localhost:7071/api/HttpStart'</code>
        /// <returns></returns>
        [FunctionName("HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Orchestration", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("Orchestration")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("SayHello-Activity", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("SayHello-Activity", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("SayHello-Activity", "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("SayHello-Activity")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

      
    }
}
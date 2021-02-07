using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FunctionApps.DurableFunctons
{
    public static class FanOutDurableFunctionExample
    {  
        /// <summary>
        /// Entry point for the durable function 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="starter"></param>
        /// <param name="log"></param>
        /// <code>curl --location --request GET 'http://localhost:7071/api/HttpStart'</code>
        /// <returns></returns>
        [FunctionName("HttpFanOutStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("FanOut-Orchestration", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("FanOut-Orchestration")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var parallelTasks = new List<Task<string>>
            {
                context.CallActivityAsync<string>("FanOut-SayHello-Activity", "Tokyo"),
                context.CallActivityAsync<string>("FanOut-SayGoodBye-Activity", "Seattle"),
                context.CallActivityAsync<string>("SayHello-Activity", "London")
            };

            await Task.WhenAll(parallelTasks);

            var outputs = parallelTasks.Select(t => t.Result).ToList();

            return outputs;
        }

        [FunctionName("FanOut-SayHello-Activity")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("FanOut-SayGoodBye-Activity")]
        public static string SayGoodBye([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying goodbye to {name}.");
            return $"Bye {name}!";
        }
    }
}
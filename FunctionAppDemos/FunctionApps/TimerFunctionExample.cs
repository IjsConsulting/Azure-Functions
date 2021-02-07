using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FunctionApps
{
    public static class TimerFunctionExample
    {
        [FunctionName("TimerTriggerFunction")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, 
            [Queue("my-mq", Connection = "queue-connection")] out string queueItem,
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            queueItem = $"This was run at: {DateTime.Now:U}";
        }
    }
}

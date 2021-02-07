using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FunctionApps
{
    public static class QueueFunctionExample
    {
        [FunctionName("QueueFunctionExample")]
        public static void Run([QueueTrigger("my-mq", Connection = "queue-connection")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }

        [FunctionName("QueueToBlobFunctionExample")]
        public static void RunA([QueueTrigger("my-mq", Connection = "queue-connection")]
            string myQueueItem,
            [Blob("input-blob/{sys.randguid}", FileAccess.Write, Connection = "queue-connection")] out string imageSmall,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            imageSmall = $"{myQueueItem}";
        }
    }
}

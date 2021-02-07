using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FunctionApps
{
    public static class BlobFunctionExample
    {
        /// <summary>
        /// Blob Function Example
        /// </summary>
        /// <param name="myBlob"></param>
        /// <param name="name"></param>
        /// <param name="log"></param>
        [FunctionName("BlogFunctionExample")]
        public static void Run(
            [BlobTrigger("input-blob/{name}.log", Connection = "blob-connection-string")]
            Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}

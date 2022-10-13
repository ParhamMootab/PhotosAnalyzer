using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PhotosAnalyzer.Models;

namespace PhotosAnalyzer
{
    public static class PhotosOrchestrator
    {

        [FunctionName("PhotosOrchestrator_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var body = await req.Content.ReadAsStringAsync();

            var jsonObj = JsonConvert.DeserializeObject<PhotoJson>(body);
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("PhotosOrchestrator", jsonObj);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("PhotosOrchestrator")]
        public static async Task<dynamic> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var model = context.GetInput<PhotoJson>(); 
            var photoBytes = await context.CallActivityAsync<byte[]>("PhotosStorage", model);
            var analysis = await context.CallActivityAsync<dynamic>("PhotosAnalyzer", photoBytes.ToList());
            return analysis;
        }

        
    }
}
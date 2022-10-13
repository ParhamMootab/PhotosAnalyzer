using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Client;
using System.Linq;
using Microsoft.Azure.Documents.Linq;
using System.Collections.Generic;
using PhotosAnalyzer.Models;

namespace PhotosAnalyzer
{
    public static class PhotosSearch
    {
        [FunctionName("PhotosSearch")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [CosmosDB("images", "metadata", ConnectionStringSetting = Literals.CosmosConnectionString)] DocumentClient client,
            ILogger logger)
        {
            logger?.LogInformation("Searching...");

            var searchTerm = req.Query["searchTerm"];
            if (string.IsNullOrEmpty(searchTerm))
            {
                return new NotFoundResult();

            }
            var uri = UriFactory.CreateDocumentCollectionUri("images", "metadata");

            var query = client.CreateDocumentQuery<PhotoJson>(uri,
                new FeedOptions() { EnableCrossPartitionQuery = true })
                .Where(p => p.Description.Contains(searchTerm))
                .AsDocumentQuery();
            var list = new List<dynamic>();

            while (query.HasMoreResults)
            {
                foreach (var doc in await query.ExecuteNextAsync())
                {
                    list.Add(doc);
                }
            }
            return new OkObjectResult(list);
        }
    }
}

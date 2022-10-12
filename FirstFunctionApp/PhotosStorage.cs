using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using PhotosAnalyzer.Models;
using PhotosAnalyzer.AnalyzerService.Abstarctions;

namespace PhotosAnalyzer
{
    public class PhotosStorage
    {
        private readonly IAnalyzerService analyzerService;

        public PhotosStorage(IAnalyzerService analyzerService)
        {
            this.analyzerService = analyzerService;
        }

        [FunctionName("PhotosStorage")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Blob("images", FileAccess.ReadWrite, Connection = Literals.BlobConnectionString)] BlobContainerClient blobContainer,
            [CosmosDB(
                "images",
                "metadata",
                ConnectionStringSetting = Literals.CosmosConnectionString,
                CreateIfNotExists = true
            )] IAsyncCollector<dynamic> items,
            ILogger logger)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<PhotoJson>(body);

            var newId = Guid.NewGuid();
            string blobName = $"{newId}.jpg";

            await blobContainer.CreateIfNotExistsAsync();
            var photoBytes = Convert.FromBase64String(data?.Photo);
            var blobClient = blobContainer.GetBlobClient($"{blobName}");
            using (var stream = new MemoryStream(photoBytes, false))
            {
                await blobClient.UploadAsync(stream);
            }

            var analysisResult = await analyzerService.AnalyzeAsync(photoBytes);

            var item = new
            {
                id = newId,
                name = data.Name,
                description = data.Description,
                tags = data.Tags,
                analysis = analysisResult

            };

            await items.AddAsync(item);

            logger.LogInformation($"Successfully uploaded {blobName} and its metadata");
            return new OkObjectResult(blobName);
        }
    }
}

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
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace PhotosAnalyzer
{
    public class PhotosStorage
    {
        

        [FunctionName("PhotosStorage")]
        public async Task<byte[]> Run(
            [ActivityTrigger] PhotoJson req,
            [Blob("images", FileAccess.ReadWrite, Connection = Literals.BlobConnectionString)] BlobContainerClient blobContainer,
            [CosmosDB(
                "images",
                "metadata",
                ConnectionStringSetting = Literals.CosmosConnectionString,
                CreateIfNotExists = true
            )] IAsyncCollector<dynamic> items,
            ILogger logger)
        {
            var newId = Guid.NewGuid();
            string blobName = $"{newId}.jpg";

            await blobContainer.CreateIfNotExistsAsync();
            var photoBytes = Convert.FromBase64String(req?.Photo);
            var blobClient = blobContainer.GetBlobClient($"{blobName}");
            using (var stream = new MemoryStream(photoBytes, false))
            {
                await blobClient.UploadAsync(stream);
            }

            var item = new
            {
                id = newId,
                name = req.Name,
                description = req.Description,
                tags = req.Tags

            };

            await items.AddAsync(item);

            logger.LogInformation($"Successfully uploaded {blobName} and its metadata");
            return photoBytes;
        }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PhotosAnalyzer
{
    public static class PhotosDownload
    {
        [FunctionName("PhotosDownload")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "photos/{id}")] HttpRequest req,
            [Blob("photos-small/{id}.jpg", FileAccess.Read, Connection = Literals.BlobConnectionString)] Stream smallImage,
            [Blob("photos-medium/{id}.jpg", FileAccess.Read, Connection = Literals.BlobConnectionString)] Stream mediumImage,
            [Blob("images/{id}.jpg", FileAccess.Read, Connection = Literals.BlobConnectionString)] Stream originalImage,
            Guid id,
            ILogger logger)
        {
            byte[] data;

            if (req.Query["size"] == "sm")
            {
                logger.LogInformation("Retrieving the small size");
                data = await GetBytesFromStreamAsync(smallImage);
            }
            else if (req.Query["size"] == "md")
            {
                logger.LogInformation("Retrieving the medium size");
                data = await GetBytesFromStreamAsync(mediumImage);
            }
            else
            {
                logger.LogInformation("Retrieving the original size");
                data = await GetBytesFromStreamAsync(originalImage);
            }

            return new FileContentResult(data, "image/jpeg")
            {
                FileDownloadName = $"{id}.jpg"
            };
        }
        private static async Task<byte[]> GetBytesFromStreamAsync(Stream stream)
        {
            var bytes = new byte[stream.Length];
            await stream.ReadAsync(bytes, 0, bytes.Length);
            return bytes;
        }
    }
}

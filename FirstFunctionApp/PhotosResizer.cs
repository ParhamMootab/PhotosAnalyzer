using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Threading.Tasks;

namespace PhotosAnalyzer
{
    public class PhotosResizer
    {
        [FunctionName("PhotosResizer")]
        public static async Task Run([BlobTrigger("images/{name}", Connection = Literals.BlobConnectionString)] Stream myBlob, string name,
            [Blob("photos-small/{name}", FileAccess.Write, Connection = Literals.BlobConnectionString)] BlobClient imageSmall,
            [Blob("photos-medium/{name}", FileAccess.Write, Connection = Literals.BlobConnectionString)] BlobClient imageMedium
            , ILogger logger)
        {
            logger?.LogInformation($"uploading to blob {name}");
            try
            {
                IImageFormat format;

                using (Image<Rgba32> input = Image.Load<Rgba32>(myBlob, out format))
                {
                    var msMedium = CreateMemoryStream(input, ImageSize.Medium, format);
                    await imageMedium.UploadAsync(msMedium, true);

                }

                myBlob.Position = 0;
                using (Image<Rgba32> input = Image.Load<Rgba32>(myBlob, out format))
                {

                    Stream smallStream = CreateMemoryStream(input, ImageSize.Small, format);

                    await imageSmall.UploadAsync(smallStream, true);
                }

            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());

            }
        }

        private static Stream CreateMemoryStream(Image<Rgba32> img, ImageSize imageSize, IImageFormat format)
        {
            var output = new MemoryStream();
            var desiredWidth = imageSize == ImageSize.Medium ? img.Width / 2 : img.Width / 4;

            var ratio = (decimal)desiredWidth / img.Width;
            img.Mutate(x => x.Resize(desiredWidth, (int)Math.Floor(img.Height * ratio)));
            img.Save(output, format);
            output.Position = 0;
            return output;
        }

        private enum ImageSize
        {
            Medium, Small
        }
    }
}

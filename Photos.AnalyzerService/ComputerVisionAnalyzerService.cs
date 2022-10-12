using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using PhotosAnalyzer.AnalyzerService.Abstarctions;

namespace PhotosAnalyzer.AnalyzerService
{
    public class ComputerVisionAnalyzerService : IAnalyzerService
    {

        private readonly ComputerVisionClient client;
        public ComputerVisionAnalyzerService(IConfiguration configuration)
        {
            var visionKey = configuration["VisionKey"];
            var visionEndpoint = configuration["VisionEndpoint"];
            client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(visionKey))
            {
                Endpoint = visionEndpoint
            };
        }

        public async Task<dynamic> AnalyzeAsync(byte[] image)
        {
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Tags
            };

            using (var ms = new MemoryStream(image))
            {
                var imageAnalysis = await client.AnalyzeImageInStreamAsync(ms, visualFeatures: features);
                var result = new
                {
                    metadata = new
                    {
                        width = imageAnalysis.Metadata.Width,
                        height = imageAnalysis.Metadata.Height,
                        format = imageAnalysis.Metadata.Format
                    },
                    categories = imageAnalysis.Tags.Select(p => p.Name).ToArray()
                };
                return result;
            }
            
            
        }
    }
}
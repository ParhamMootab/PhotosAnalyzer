using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using PhotosAnalyzer.AnalyzerService.Abstarctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotosAnalyzer
{
    public class PhotosAnalyzer
    {
        private readonly IAnalyzerService analyzerService;

        public PhotosAnalyzer(IAnalyzerService analyzerService)
        {
            this.analyzerService = analyzerService;
        }

        [FunctionName("PhotosAnalyzer")]
        public async Task<dynamic> Run([ActivityTrigger] List<byte> image)
        {
            return await analyzerService.AnalyzeAsync(image.ToArray());
        }
    }
}

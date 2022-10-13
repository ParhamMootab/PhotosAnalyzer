using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using PhotosAnalyzer;
using PhotosAnalyzer.AnalyzerService;
using PhotosAnalyzer.AnalyzerService.Abstarctions;

[assembly: FunctionsStartup(typeof(Startup))]

namespace PhotosAnalyzer
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IAnalyzerService, ComputerVisionAnalyzerService>();
        }
    }
}

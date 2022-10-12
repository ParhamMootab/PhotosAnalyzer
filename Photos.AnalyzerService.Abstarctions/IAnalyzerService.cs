namespace PhotosAnalyzer.AnalyzerService.Abstarctions
{
    public interface IAnalyzerService
    {
        public Task<dynamic> AnalyzeAsync(byte[] image);
    }
}
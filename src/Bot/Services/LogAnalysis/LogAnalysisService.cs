namespace RyuBot.Services;

public class LogAnalysisService : BotService
{
    private readonly HttpClient _httpClient;
    
    public LogAnalysisService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async ValueTask<LogAnalysis> DownloadLogAsync(Attachment attachment)
    {
        var text = await _httpClient.GetStringAsync(attachment.ProxyUrl);

        return new LogAnalysis(text, this);
    }
    
    public LogAnalysis AnalyzeLog(string logFileText) => new(logFileText, this);
}
namespace RyuBot.Services;

public class CompatibilityCsvService : BotService
{
    private readonly HttpClient _client;
    
    public CompatibilityCsvService(HttpClient httpClient)
    {
        _client = httpClient;
    }
    
    private static readonly FilePath _csvPath = FilePath.Data / "compatibility.csv";
    
    private static readonly string _downloadUrl =
        "https://gist.githubusercontent.com/ezhevita/b41ed3bf64d0cc01269cab036e884f3d/raw/002b1a1c1a5f7a83276625e8c479c987a5f5b722/Ryujinx%2520Games%2520List%2520Compatibility.csv";

    public CompatibilityCsv Csv { get; private set; }
    
    
    public async Task InitAsync()
    {
        try
        {
            var text = await _client.GetStringAsync(_downloadUrl);
            Info(LogSource.Service, "Compatibility CSV downloaded.");

            Csv = new CompatibilityCsv(text);
            _csvPath.WriteAllText(text);
        }
        catch
        {
            if (_csvPath.ExistsAsFile)
            {
                Info(LogSource.Service, "Request to get compatibility CSV failed; using previous version.");
                var existingCsv = _csvPath.ReadAllText();
                Csv = new CompatibilityCsv(existingCsv);
            }
        }
    }
}
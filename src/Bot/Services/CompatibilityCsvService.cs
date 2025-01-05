namespace RyuBot.Services;

public class CompatibilityCsvService : BotService
{
    private readonly HttpClient _client;

    public CompatibilityCsvService(HttpClient httpClient)
    {
        _client = httpClient;
    }

    private static readonly FilePath CsvPath = FilePath.Data / "compatibility.csv";

    private const string DownloadUrl =
        "https://gist.githubusercontent.com/ezhevita/" +
        "b41ed3bf64d0cc01269cab036e884f3d/raw/002b1a1c1a5f7a83276625e8c479c987a5f5b722/" +
        "Ryujinx%2520Games%2520List%2520Compatibility.csv";

    public CompatibilityCsv Csv { get; private set; }

    public CompatibilityCsv.Entry GetByGameName(string name)
        => Csv.Entries.FirstOrDefault(x => x.GameName.EqualsIgnoreCase(name));

    public CompatibilityCsv.Entry GetByTitleId(string titleId)
        => Csv.Entries.FirstOrDefault(x => x.TitleId == titleId);

    public Gommon.Optional<CompatibilityCsv.Entry> FindOrNull(string nameOrTitleId)
        => Csv.Entries.FindFirst(x => 
            x.GameName.EqualsIgnoreCase(nameOrTitleId) || 
            x.TitleId.Check(it => it.EqualsIgnoreCase(nameOrTitleId))
        );

    public IEnumerable<CompatibilityCsv.Entry> SearchEntries(string nameOrTitleId) =>
        Csv.Entries.Where(x => 
            x.GameName.ContainsIgnoreCase(nameOrTitleId) || 
            x.TitleId.Check(it => it.ContainsIgnoreCase(nameOrTitleId))
        );


    public async Task InitAsync()
    {
        try
        {
            var text = await _client.GetStringAsync(DownloadUrl);
            Info(LogSource.Service, "Compatibility CSV downloaded.");

            Csv = new CompatibilityCsv(text);
            CsvPath.WriteAllText(text);
        }
        catch
        {
            if (CsvPath.ExistsAsFile)
            {
                Info(LogSource.Service, "Request to get compatibility CSV failed; using previous version.");
                var existingCsv = CsvPath.ReadAllText();
                Csv = new CompatibilityCsv(existingCsv);
            }
        }
    }
}
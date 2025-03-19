using nietras.SeparatedValues;

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
        "https://git.ryujinx.app/ryubing/ryujinx/-/raw/master/docs/compatibility.csv?ref_type=heads&inline=false";

    public CompatibilityCsv Csv { get; private set; }

    public CompatibilityEntry GetByGameName(string name)
        => Csv.Entries.FirstOrDefault(x => x.GameName.EqualsIgnoreCase(name));

    public CompatibilityEntry GetByTitleId(string titleId)
        => Csv.Entries.FirstOrDefault(x => x.TitleId == titleId);

    public Gommon.Optional<CompatibilityEntry> FindOrNull(string nameOrTitleId)
        => Csv.Entries.FindFirst(x => 
            x.GameName.EqualsIgnoreCase(nameOrTitleId) || 
            x.TitleId.Check(it => it.EqualsIgnoreCase(nameOrTitleId))
        );

    public IEnumerable<CompatibilityEntry> SearchEntries(string nameOrTitleId) =>
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

            Csv = new CompatibilityCsv(Sep.Reader().FromText(text));
            CsvPath.WriteAllText(text);
        }
        catch
        {
            if (CsvPath.ExistsAsFile)
            {
                Info(LogSource.Service, "Request to get compatibility CSV failed; using previous version.");
                Csv = new CompatibilityCsv(Sep.Reader().FromFile(CsvPath.Path));
            }
        }
    }
}
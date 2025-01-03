namespace RyuBot.Entities;

public class CompatibilityCsv
{
    public CompatibilityCsv(string entireCsv)
    {
        Entries = entireCsv.Split('\n')
            .Skip(1) //CSV format
            .Select(it => new CsvEntry(it.Split(',')))
            .ToArray();
    }
    
    public CsvEntry[] Entries { get; set; }
}

public class CsvEntry
{
    // issue_number,issue_title,extracted_game_id,issue_labels,extracted_status,last_event_date,events_count

    public CsvEntry(string[] rawCsvEntry)
    {
        _raw = rawCsvEntry;
    }
    
    private readonly string[] _raw;

    public int IssueNumber => int.Parse(_raw[0]);
    public string GameName => _raw[1]?.Split("-")?.FirstOrDefault()?.Trim();
    public string TitleId => _raw[2];
    public string IssueLabels => _raw[3];
    public string PlayabilityStatus => _raw[4];
    public string LastEvent => _raw[5];
    public int EventCount => int.Parse(_raw[6]);

    public override string ToString()
    {
        var sb = new StringBuilder("CompatibilityCsvEntry: {");
        sb.Append($"{nameof(IssueNumber)}={IssueNumber}");
        sb.Append(',');
        sb.Append($"{nameof(GameName)}=\"{GameName}\"");
        sb.Append(',');
        sb.Append($"{nameof(TitleId)}={TitleId}");
        sb.Append(',');
        sb.Append($"{nameof(IssueLabels)}=\"{IssueLabels}\"");
        sb.Append(',');
        sb.Append($"{nameof(PlayabilityStatus)}=\"{PlayabilityStatus}\"");
        sb.Append(',');
        sb.Append($"{nameof(LastEvent)}=\"{LastEvent}\"");
        sb.Append(',');
        sb.Append($"{nameof(EventCount)}={EventCount}");
        sb.Append('}');

        return sb.ToString();
    }
}
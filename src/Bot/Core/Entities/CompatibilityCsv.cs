namespace RyuBot.Entities;

public class CompatibilityCsv
{
    public CompatibilityCsv(string entireCsv)
    {
        Entries = entireCsv.Split('\n')
            .Skip(1) //CSV format
            .Select(it => new CompatibilityEntry(it.Split(',')))
            .ToArray();
    }

    public CompatibilityEntry[] Entries { get; set; }
}

public class CompatibilityEntry
{
    // ReSharper disable InconsistentNaming
    private const int issue_number = 0;
    private const int issue_title = 1;
    private const int extracted_game_id = 2;
    private const int issue_labels = 3;
    private const int extracted_status = 4;
    private const int last_event_date = 5;
    private const int events_count = 6;
    // ReSharper restore InconsistentNaming

    private readonly string[] _raw;

    // ReSharper disable InconsistentNaming
    private readonly Lazy<int> l_IssueNumber;
    private readonly Lazy<string> l_GameName;
    private readonly Lazy<Gommon.Optional<string>> l_TitleId;
    private readonly Lazy<string[]> l_IssueLabels;
    private readonly Lazy<string> l_Status;
    private readonly Lazy<DateTime> l_LastEvent;
    private readonly Lazy<int> l_EventCount;
    // ReSharper restore InconsistentNaming

    public CompatibilityEntry(string[] rawCsvEntry)
    {
        _raw = rawCsvEntry;

        l_IssueNumber = new(() => int.Parse(_raw[issue_number]));
        l_GameName = new(() => _raw[issue_title].Split("-")[0].Trim());
        l_TitleId = new(() =>
        {
            var tid = _raw[extracted_game_id];
            return tid == string.Empty
                ? default(Gommon.Optional<string>)
                : tid;
        });
        l_IssueLabels = new(() => _raw[issue_labels].Split(';'));
        l_Status = new(() => _raw[extracted_status]);
        l_LastEvent = new(() => DateTime.TryParse(_raw[last_event_date], out var time) ? time : default);
        l_EventCount = new(() => int.TryParse(_raw[events_count], out var count) ? count : -1);
    }

    public int IssueNumber => l_IssueNumber.Value;
    public string GameName => l_GameName.Value;
    public Gommon.Optional<string> TitleId => l_TitleId.Value;
    public string[] IssueLabels => l_IssueLabels.Value;
    public string Status => l_Status.Value.Replace("status-", string.Empty).Capitalize();
    public DateTime LastEvent => l_LastEvent.Value;
    public int EventCount => l_EventCount.Value;

    public string FormattedIssueLabels => FormatIssueLabels(false);

    public string FormatIssueLabels(bool markdown = true) => IssueLabels
        .Where(it => !it.StartsWithIgnoreCase("status"))
        .Select(it => GitHubHelper.FormatLabelName(it, markdown))
        .JoinToString(", ");

    public override string ToString()
    {
        var sb = new StringBuilder("CompatibilityCsv.Entry: {");
        sb.Append($"{nameof(IssueNumber)}={IssueNumber}");
        sb.Append(',');
        sb.Append($"{nameof(GameName)}=\"{GameName}\"");
        sb.Append(',');
        sb.Append($"{nameof(TitleId)}={TitleId}");
        sb.Append(',');
        sb.Append($"{nameof(IssueLabels)}=\"{IssueLabels}\"");
        sb.Append(',');
        sb.Append($"{nameof(Status)}=\"{Status}\"");
        sb.Append(',');
        sb.Append($"{nameof(LastEvent)}=\"{LastEvent}\"");
        sb.Append(',');
        sb.Append($"{nameof(EventCount)}={EventCount}");
        sb.Append('}');

        return sb.ToString();
    }
}
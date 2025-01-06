using nietras.SeparatedValues;

namespace RyuBot.Entities;

public class CompatibilityCsv
{
    public CompatibilityCsv(SepReader reader)
    {
        var entries = new List<CompatibilityEntry>();

        foreach (var row in reader)
        {
            entries.Add(new CompatibilityEntry(reader.Header, row));
        }

        Entries = entries.Where(x => !x.Status.IsNullOrEmpty())
            .OrderBy(it => it.GameName).ToArray();
    }

    public CompatibilityEntry[] Entries { get; }
}

public class CompatibilityEntry
{
    public CompatibilityEntry(SepReaderHeader header, SepReader.Row row)
    {
        IssueNumber = row[header.IndexOf("issue_number")].Parse<int>();
        
        var titleIdRow = row[header.IndexOf("extracted_game_id")].ToString();
        if (!string.IsNullOrEmpty(titleIdRow))
            TitleId = titleIdRow;

        var issueTitleRow = row[header.IndexOf("issue_title")].ToString();
        if (TitleId.HasValue)
            issueTitleRow = issueTitleRow.ReplaceIgnoreCase($" - {TitleId}", string.Empty);
        
        GameName = issueTitleRow.Trim().Trim('"');

        IssueLabels = row[header.IndexOf("issue_labels")].ToString().Split(';');
        Status = row[header.IndexOf("extracted_status")].ToString().Capitalize();
        
        if (row[header.IndexOf("last_event_date")].TryParse<DateTime>(out var dt))
            LastEvent = dt;

        if (row[header.IndexOf("events_count")].TryParse<int>(out var eventsCount))
            EventCount = eventsCount;
    }

    public int IssueNumber { get; }
    public string GameName { get; }
    public Gommon.Optional<string> TitleId { get; }
    public string[] IssueLabels { get; }
    public string Status { get; }
    public DateTime LastEvent { get; }
    public int EventCount { get; }

    public string FormattedTitleId => TitleId.OrElse(new string(' ', 16));

    public string FormattedIssueLabels => FormatIssueLabels(false);

    public string FormatIssueLabels(bool markdown = true) => IssueLabels
        .Where(it => !it.StartsWithIgnoreCase("status"))
        .Select(it => GitHubHelper.FormatLabelName(it, markdown))
        .JoinToString(", ");

    public override string ToString()
    {
        var sb = new StringBuilder("CompatibilityCsv.Entry: {");
        sb.Append($"{nameof(IssueNumber)}={IssueNumber}, ");
        sb.Append($"{nameof(GameName)}=\"{GameName}\", ");
        sb.Append($"{nameof(TitleId)}={TitleId}, ");
        sb.Append($"{nameof(IssueLabels)}=\"{IssueLabels}\", ");
        sb.Append($"{nameof(Status)}=\"{Status}\", ");
        sb.Append($"{nameof(LastEvent)}=\"{LastEvent}\", ");
        sb.Append($"{nameof(EventCount)}={EventCount}");
        sb.Append('}');

        return sb.ToString();
    }
}
using System.IO;
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
        if (row.ColCount != header.ColNames.Count)
            throw new InvalidDataException($"CSV row {row.RowIndex} ({row.ToString()}) has mismatched column count");
            
        var titleIdRow = colStr(row[header.IndexOf("\"extracted_game_id\"")]);
        TitleId = !string.IsNullOrEmpty(titleIdRow) 
            ? titleIdRow 
            : default(Gommon.Optional<string>);

        var issueTitleRow = colStr(row[header.IndexOf("\"issue_title\"")]);
        if (TitleId.HasValue)
            issueTitleRow = issueTitleRow.ReplaceIgnoreCase($" - {TitleId}", string.Empty);

        GameName = issueTitleRow.Trim().Trim('"');

        IssueLabels = colStr(row[header.IndexOf("\"issue_labels\"")]).Split(';');
        Status = colStr(row[header.IndexOf("\"extracted_status\"")]).Capitalize();

        if (DateTime.TryParse(colStr(row[header.IndexOf("\"last_event_date\"")]), out var dt))
            LastEvent = dt;

        return;
            
        string colStr(SepReader.Col col) => col.ToString().Trim('"');
    }
    
    public string GameName { get; }
    public Gommon.Optional<string> TitleId { get; }
    public string[] IssueLabels { get; }
    public string Status { get; }
    public DateTime LastEvent { get; }

    public string FormattedTitleId => TitleId.OrElse(new string(' ', 16));

    public string FormattedIssueLabels => FormatIssueLabels(false);

    public string FormatIssueLabels(bool markdown = true) => IssueLabels
        .Where(it => !it.StartsWithIgnoreCase("status"))
        .Select(it => GitHubHelper.FormatLabelName(it, markdown))
        .JoinToString(", ");

    public override string ToString()
    {
        var sb = new StringBuilder($"{nameof(CompatibilityEntry)}: {{");
        sb.Append($"{nameof(GameName)}=\"{GameName}\", ");
        sb.Append($"{nameof(TitleId)}={TitleId}, ");
        sb.Append($"{nameof(IssueLabels)}=\"{IssueLabels}\", ");
        sb.Append($"{nameof(Status)}=\"{Status}\", ");
        sb.Append($"{nameof(LastEvent)}=\"{LastEvent}\"");
        sb.Append('}');

        return sb.ToString();
    }
}
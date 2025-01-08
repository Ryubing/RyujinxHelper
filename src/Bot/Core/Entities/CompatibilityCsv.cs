using System.Globalization;
using System.IO;
using nietras.SeparatedValues;

namespace RyuBot.Entities;

public class CompatibilityCsv
{
    public const string ExtractedGameIdColumn = "\"extracted_game_id\"";
    public const string IssueTitleColumn = "\"issue_title\"";
    public const string IssueLabelsColumn = "\"issue_labels\"";
    public const string ExtractedStatusColumn = "\"extracted_status\"";
    public const string LastEventDateColumn = "\"last_event_date\"";
    
    private readonly SepReader _reader;
    
    public CompatibilityCsv(SepReader reader)
    {
        _reader = reader;
        var entries = new List<CompatibilityEntry>();

        foreach (var row in reader)
        {
            entries.Add(new CompatibilityEntry(reader.Header, row));
        }

        Entries = entries.Where(x => !x.Status.IsNullOrEmpty())
            .OrderBy(it => it.GameName).ToArray();
    }

    public CompatibilityEntry[] Entries { get; }


    public void Export(FilePath newPath)
    {
        var sepWriter = _reader.Spec.Writer().ToText();

        foreach (var compatEntry in Entries)
        {
            using var row = sepWriter.NewRow();
            row[IssueTitleColumn].Set($"\"{compatEntry.GameName}\"");
            row[ExtractedGameIdColumn].Set(compatEntry.TitleId.OrElse(string.Empty));
            row[IssueLabelsColumn].Set(compatEntry.IssueLabels.JoinToString(';'));
            row[ExtractedStatusColumn].Set(compatEntry.Status.ToLower());
            var le = compatEntry.LastEvent;
            row[LastEventDateColumn].Set(
                $"{le.Year}-{le.Month:00}-{le.Day:00} " +
                $"{le.Hour:00}:{le.Minute:00}:{le.Second:00}.000"
                );
        }

        if (newPath.TryGetParent(out var parent) && !parent.ExistsAsDirectory)
            Directory.CreateDirectory(parent.Path);
        
        newPath.WriteAllText(sepWriter.ToString());
    }
}

public class CompatibilityEntry
{
    public CompatibilityEntry(SepReaderHeader header, SepReader.Row row)
    {
        if (row.ColCount != header.ColNames.Count)
            throw new InvalidDataException($"CSV row {row.RowIndex} ({row.ToString()}) has mismatched column count");
            
        var titleIdRow = colStr(row[header.IndexOf(CompatibilityCsv.ExtractedGameIdColumn)]);
        TitleId = !string.IsNullOrEmpty(titleIdRow) 
            ? titleIdRow 
            : default(Gommon.Optional<string>);

        var issueTitleRow = colStr(row[header.IndexOf(CompatibilityCsv.IssueTitleColumn)]);
        if (TitleId.HasValue)
            issueTitleRow = issueTitleRow.ReplaceIgnoreCase($" - {TitleId}", string.Empty);

        GameName = issueTitleRow.Trim().Trim('"');

        IssueLabels = colStr(row[header.IndexOf(CompatibilityCsv.IssueLabelsColumn)]).Split(';');
        Status = colStr(row[header.IndexOf(CompatibilityCsv.ExtractedStatusColumn)]).Capitalize();

        if (DateTime.TryParse(colStr(row[header.IndexOf(CompatibilityCsv.LastEventDateColumn)]), out var dt))
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
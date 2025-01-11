using System.IO;
using nietras.SeparatedValues;

namespace RyuBot.Entities;

public struct ColumnIndices(Func<ReadOnlySpan<char>, int> getIndex)
{
    public const string TitleIdCol = "\"title_id\"";
    public const string GameNameCol = "\"game_name\"";
    public const string LabelsCol = "\"labels\"";
    public const string StatusCol = "\"status\"";
    public const string LastUpdatedCol = "\"last_updated\"";

    public readonly int TitleId = getIndex(TitleIdCol);
    public readonly int GameName = getIndex(GameNameCol);
    public readonly int Labels = getIndex(LabelsCol);
    public readonly int Status = getIndex(StatusCol);
    public readonly int LastUpdated = getIndex(LastUpdatedCol);
}

public class CompatibilityCsv
{
    private readonly SepSpec _spec;

    public CompatibilityCsv(SepReader reader)
    {
        _spec = reader.Spec;
        ColumnIndices columnIndices = new(reader.Header.IndexOf);

        Entries = reader
            .Enumerate(row => new CompatibilityEntry(ref columnIndices, row))
            .OrderBy(it => it.GameName)
            .ToArray();
    }

    public CompatibilityEntry[] Entries { get; }


    public void Export(FilePath newPath)
    {
        var sepWriter = _spec.Writer().ToText();

        foreach (var compatEntry in Entries)
        {
            using var row = sepWriter.NewRow();
            row[ColumnIndices.TitleIdCol].Set(compatEntry.TitleId.OrElse(string.Empty));
            row[ColumnIndices.GameNameCol].Set($"\"{compatEntry.GameName}\"");
            row[ColumnIndices.LabelsCol].Set(
                compatEntry.Labels
                .Where(x => !x.StartsWithIgnoreCase("status"))
                .JoinToString(';')
            );
            row[ColumnIndices.StatusCol].Set(compatEntry.Status.ToLower());
            var le = compatEntry.LastEvent;
            row[ColumnIndices.LastUpdatedCol].Set(
                $"{le.Year}-{le.Month:00}-{le.Day:00} " +
                $"{le.Hour:00}:{le.Minute:00}:{le.Second:00}"
            );
        }

        if (newPath.TryGetParent(out var parent) && !parent.ExistsAsDirectory)
            Directory.CreateDirectory(parent.Path);

        newPath.WriteAllText(sepWriter.ToString());
    }
}

public class CompatibilityEntry
{
    public CompatibilityEntry(ref ColumnIndices indices, SepReader.Row row)
    {
        var titleIdRow = colStr(row[indices.TitleId]);
        TitleId = !string.IsNullOrEmpty(titleIdRow)
            ? titleIdRow
            : default(Gommon.Optional<string>);

        GameName = colStr(row[indices.GameName]).Trim().Trim('"');

        var labelsStr = colStr(row[indices.Labels]);
        Labels = !string.IsNullOrWhiteSpace(labelsStr) 
            ? labelsStr.Split(';') 
            : [];
        
        Status = colStr(row[indices.Status]).Capitalize();

        if (DateTime.TryParse(colStr(row[indices.LastUpdated]), out var dt))
            LastEvent = dt;

        return;

        string colStr(SepReader.Col col) => col.ToString().Trim('"');
    }

    public string GameName { get; }
    public Gommon.Optional<string> TitleId { get; }
    public string[] Labels { get; }
    public string Status { get; }
    public DateTime LastEvent { get; }

    public string FormattedTitleId => TitleId.OrElse(new string(' ', 16));

    public string FormattedIssueLabels => FormatIssueLabels(false);

    public string FormatIssueLabels(bool markdown = true) =>
        Labels
            .FormatCollection(
                it => GitHubHelper.FormatLabelName(it, markdown),
                separator: ", "
            );

    public override string ToString()
    {
        var sb = new StringBuilder($"{nameof(CompatibilityEntry)}: {{");
        if (TitleId.HasValue)
            sb.Append($"{nameof(TitleId)}={TitleId}, ");
        sb.Append($"{nameof(GameName)}=\"{GameName}\", ");
        sb.Append($"{nameof(Labels)}={
            Labels.FormatCollection(x => $"\"{x}\"", separator: ", ", prefix: "[", suffix: "]")
        }, ");
        sb.Append($"{nameof(Status)}=\"{Status}\", ");
        sb.Append($"{nameof(LastEvent)}=\"{LastEvent}\"");
        sb.Append('}');

        return sb.ToString();
    }
}
namespace RyuBot.Entities;

public class CompatibilityCsv
{
    public CompatibilityCsv(string entireCsv)
    {
        Entries = entireCsv.Split('\n')
            .Skip(1) //CSV format
            .Select(it => new Entry(it.Split(',')))
            .ToArray();
    }
    
    public Entry[] Entries { get; set; }
    
    public class Entry
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
        
        public Entry(string[] rawCsvEntry)
        {
            _raw = rawCsvEntry;
        }
        
        public int IssueNumber => int.Parse(_raw[issue_number]);
        public string GameName => _raw[issue_title]?.Split("-")?.FirstOrDefault()?.Trim();
        public string TitleId => _raw[extracted_game_id];
        public string IssueLabels => _raw[issue_labels];
        public string PlayabilityStatus => _raw[extracted_status];
        public DateTime LastEvent => DateTime.Parse(_raw[last_event_date]);
        public int EventCount => int.Parse(_raw[events_count]);
    
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
            sb.Append($"{nameof(PlayabilityStatus)}=\"{PlayabilityStatus}\"");
            sb.Append(',');
            sb.Append($"{nameof(LastEvent)}=\"{LastEvent}\"");
            sb.Append(',');
            sb.Append($"{nameof(EventCount)}={EventCount}");
            sb.Append('}');
    
            return sb.ToString();
        }
    }
}
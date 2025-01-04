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
        
        // ReSharper disable InconsistentNaming
        private readonly Lazy<int> l_IssueNumber;
        private readonly Lazy<string> l_GameName;
        private readonly Lazy<Gommon.Optional<string>> l_TitleId;
        private readonly Lazy<string[]> l_IssueLabels;
        private readonly Lazy<string> l_Status;
        private readonly Lazy<DateTime> l_LastEvent;
        private readonly Lazy<int> l_EventCount;
        // ReSharper restore InconsistentNaming
        
        public Entry(string[] rawCsvEntry)
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
            l_LastEvent = new(() => DateTime.Parse(_raw[last_event_date]));
            l_EventCount = new(() => int.Parse(_raw[events_count]));
        }
        
        public int IssueNumber => l_IssueNumber.Value;
        public string GameName => l_GameName.Value;
        public Gommon.Optional<string> TitleId => l_TitleId.Value;
        public string[] IssueLabels => l_IssueLabels.Value;
        public string Status => l_Status.Value;
        public DateTime LastEvent => l_LastEvent.Value;
        public int EventCount => l_EventCount.Value;
    
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
}
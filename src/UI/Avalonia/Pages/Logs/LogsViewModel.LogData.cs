using System.Text;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Discord;
using Gommon;
using Volte.Entities;
using Volte.Helpers;
using Console = Colorful.Console;

namespace Volte.UI.Avalonia.Pages;

public partial class VolteLog : ObservableObject
{
    private static readonly int LongestSeverity = Enum.GetValues<LogSeverity>()
        .Select(sev => Enum.GetName(sev)!.Length)
        .Max();
    
    private static readonly int LongestSource = Enum.GetValues<LogSource>()
        .Select(sev => Enum.GetName(sev)!.Length)
        .Max();
    
    private static readonly string PaddingLengthBlankSpace = new(' ', (int)(LongestSeverity * 1.33) + (int)(LongestSource * 1.85));
    
    public VolteLog(VolteLogEventArgs eventData)
    {
        Severity = eventData.Severity;
        Source = eventData.Source;
        Message = eventData.Message;
        Error = eventData.Error;
    }
    
    public LogSeverity Severity { get; init; }
    public LogSource Source { get; init; }
    public string? Message { get; init; }
    public Exception? Error { get; init; }
    
    [ObservableProperty] 
    private DateTime _date = DateTime.Now;

    public string SeverityName => Enum.GetName(Severity)!.ToUpper();
    public string SourceName => Enum.GetName(Source)!.ToUpper();

    public string String 
    {
        get
        {
            var sb = new StringBuilder();
            sb.Append($"{SeverityName}:".P((int)(LongestSeverity * 1.33)));
            sb.Append($"[{SourceName}]".P((int)(LongestSource * 1.85)));
            
            if (Message is not null)
                sb.Append(Message);

            if (Error is not null)
                sb.AppendAllLines(errorString().ReplaceLineEndings(Environment.NewLine + PaddingLengthBlankSpace).Split(Environment.NewLine));

            return sb.ToString();

            string errorString() =>
                Environment.NewLine + $"{Error!.GetType().AsFullNamePrettyString()}: " 
                                    + (Error.Message.IsNullOrEmpty() ? "No message provided" : Error.Message) +
                Environment.NewLine + Error.StackTrace;
        }
    }


    public string Markdown =>
        $"""
         `[{SeverityName}]` `[{SourceName}]` 
         `[{Date}]`

         ```
         {Message}
         ```
         """;
}
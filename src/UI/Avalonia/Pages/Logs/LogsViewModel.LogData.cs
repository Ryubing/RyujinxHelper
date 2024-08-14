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

    public string String 
    {
        get
        {
            var sb = new StringBuilder();
            sb.Append($"{Enum.GetName(Severity)!.ToUpper()}:".P(12));
            sb.Append($"[{Enum.GetName(Source)!.ToUpper()}]".P(12));
            sb.Append(Message ?? "null");

            return sb.ToString();
        }
    }


    public string Markdown =>
        $"""
         `[{Enum.GetName(Severity)!.ToUpper()}]` `[{Enum.GetName(Source)!.ToUpper()}]` 
         `[{Date}]`

         ```
         {Message}
         ```
         """;
}
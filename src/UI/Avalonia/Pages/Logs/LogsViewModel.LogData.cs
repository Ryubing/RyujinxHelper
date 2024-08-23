using System.Text;
using Discord;
using Gommon;
using Volte.Entities;
using Volte.Helpers;
using Volte.Services;
// ReSharper disable MemberCanBePrivate.Global

namespace Volte.UI.Avalonia.Pages;

public readonly record struct VolteLog
{
    private static readonly int LongestSeverity = Enum.GetValues<LogSeverity>()
        .Max(sev => Enum.GetName(sev)!.Length);

    private static readonly int LongestSource = Enum.GetValues<LogSource>()
        .Max(src => Enum.GetName(src)!.Length);
    
    private static readonly int SeverityPadding = (int)(LongestSeverity * 1.33);
    private static readonly int SourcePadding = (int)(LongestSource * 1.85);
    private static readonly string PaddingLengthBlankSpace = new(' ', SeverityPadding + SourcePadding);

    public VolteLog(VolteLogEventArgs eventData)
    {
        Severity = eventData.Severity;
        Source = eventData.Source;
        Message = eventData.Message;
        Error = eventData.Error;
    }

    public DateTime Date { get; } = DateTime.Now;

    public LogSeverity Severity { get; }
    public LogSource Source { get; }
    public string? Message { get; }
    public Exception? Error { get; }

    public string? AlignedMessage => Message?.Replace(MessageService.Whitespace, PaddingLengthBlankSpace);
    public string SeverityName => Enum.GetName(Severity)!.ToUpper();
    public string SourceName => Enum.GetName(Source)!.ToUpper();

    public string String
    {
        get
        {
            var sb = new StringBuilder();
            sb.Append($"{SeverityName}:".P(SeverityPadding));
            sb.Append($"[{SourceName}]".P(SourcePadding));

            if (Message is not null)
                sb.Append(AlignedMessage);

            if (Error is not null)
                sb.AppendAllLines(new StringBuilder()
                    .AppendLine()
                    .Append($"{Error!.GetType().AsFullNamePrettyString()}: ")
                    .AppendLine(Error.Message.IsNullOrEmpty() ? "No message provided" : Error.Message)
                    .Append(Error.StackTrace)
                    .ToString()
                    .ReplaceLineEndings(PaddingLengthBlankSpace.Prepend(Environment.NewLine))
                    .Split(Environment.NewLine)
                );

            return sb.ToString();
        }
    }


    public string Markdown =>
        $"""
         `[{SourceName}]` `[{SeverityName}]` 
         `[{Date}]`

         {Format.Code(Message, string.Empty)}
         """;
}
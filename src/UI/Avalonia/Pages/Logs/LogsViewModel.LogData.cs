﻿using System.Text;
using Discord;
using Gommon;
using RyuBot.Entities;
using RyuBot.Helpers;

// ReSharper disable MemberCanBePrivate.Global

namespace RyuBot.UI.Avalonia.Pages;

public record struct LogModel
{
    private static readonly int LongestSeverity = Enum.GetValues<LogSeverity>()
        .Max(sev => Enum.GetName(sev)!.Length);

    private static readonly int LongestSource = Enum.GetValues<LogSource>()
        .Max(src => Enum.GetName(src)!.Length);

    private static readonly int SeverityPadding = (int)(LongestSeverity * 1.33);
    private static readonly int SourcePadding = (int)(LongestSource * 1.85);

    public LogModel(LogEventArgs eventData)
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

    private StringBuilder? _formatted = null;

    public string? StrippedMessage => Message?.Replace(Logger.Whitespace, string.Empty);
    public string SeverityName => Enum.GetName(Severity)!.ToUpper();
    public string SourceName => Enum.GetName(Source)!.ToUpper();

    public string FormattedSeverityName => $"{SeverityName}:".P(SeverityPadding);
    public string FormattedSourceName => $"[{SourceName}] ->".P(SourcePadding + 3); //+3 accounts for the space and arrow 

    public string FormattedMessage
    {
        get
        {
            if (Message is not null)
                return StrippedMessage!;

            return new StringBuilder($"{Error!.GetType().AsFullNamePrettyString()}: ")
                .AppendLine(Error.Message.IsNullOrEmpty() ? "No message provided" : Error.Message)
                .Append(Error.StackTrace)
                .ToString();
        }
    }

    public string FormattedString
    {
        get
        {
            if (_formatted is not null) return _formatted.ToString();

            _formatted = new StringBuilder();
            _formatted.Append(FormattedSeverityName);
            _formatted.Append($"[{SourceName}]".P(SourcePadding));

            _formatted.Append(FormattedMessage);

            return _formatted.ToString();
        }
    }


    public string Markdown =>
        $"""
          `[{Date.FormatDate()} @ {Date.FormatFullTime()}]`
          `[{SourceName}]` `[{SeverityName}]` 

          {Format.Code(StrippedMessage, string.Empty)}
          """;
}
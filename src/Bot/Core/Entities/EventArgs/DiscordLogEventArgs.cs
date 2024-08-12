namespace Volte.Entities;

public sealed class DiscordLogEventArgs : EventArgs
{
    public string Message { get; }
    public string Source { get; }
    public LogSeverity Severity { get; }
    public LogMessage LogMessage { get; }

    public DiscordLogEventArgs(DiscordLogMessage message)
    {
        Message = message.Message;
        Source = message.Source;
        Severity = message.Severity;
        LogMessage = LogMessage.FromDiscordLogMessage(message);
    }
}
namespace Volte.Entities;

#nullable enable

public class VolteLogEventArgs
{
    public required LogSeverity Severity;
    public required LogSource Source;
    public required string Message;
    public required string[] PrintedLines;
    public required Exception? Error;
}
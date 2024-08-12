namespace Volte.Entities;

#nullable enable

public struct VolteLogEventArgs
{
    public VolteLogEventArgs() { }

    public required LogSeverity Severity { get; init; }
    public required LogSource Source { get; init; }
    public required string? Message { get; init; }
    public required Exception? Error { get; init; }
    public InvocationInfo Invocation { get; init; } = default;
}
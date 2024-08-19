namespace Volte.Entities;

public struct AppStatusEventArgs
{
    public AppStatusEventArgs()
    {
    }

    public required string Status { get; init; }
    public string Icon { get; private init; } = "fa-regular fa-message";
    public bool? IsWorkingStatus { get; private init; } = null;
    public TimeSpan? StatusExpiresAfter { get; private init; } = null;
    public LogSeverity? Severity { get; private init; } = null;


    public static event Action<AppStatusEventArgs> ChangeRequested
    {
        add => ChangeRequestedHandler.Add(value);
        remove => ChangeRequestedHandler.Remove(value);
    }

    private static readonly Event<Action<AppStatusEventArgs>> ChangeRequestedHandler = new();

    public static void SetAppStatus(
        string status,
        string icon = "fa-regular fa-message",
        bool? isWorkingStatus = null,
        TimeSpan? statusExpiresAfter = null,
        LogSeverity? severity = null
    )
        => ChangeRequestedHandler.Call(new AppStatusEventArgs
        {
            Status = status,
            Icon = icon,
            IsWorkingStatus = isWorkingStatus,
            StatusExpiresAfter = statusExpiresAfter,
            Severity = severity
        });
}
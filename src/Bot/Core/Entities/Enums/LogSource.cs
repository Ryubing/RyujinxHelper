namespace RyuBot.Entities;

public class LogSources
{
    public static LogSource Parse(string source) => source.ToLower() switch
    {
        "rest" => LogSource.Rest,
        "discord" => LogSource.Discord,
        "gateway" => LogSource.Gateway,
        "app commands" => LogSource.Discord,
        _ => LogSource.Unknown
    };
}
    
public enum LogSource
{
    /// <summary>
    ///     Indicates that this log message is from a command or module.
    /// </summary>
    Module = 1,
    /// <summary>
    ///     Indicates that this log message is from a Service.
    /// </summary>
    Service = 2,
    /// <summary>
    ///     Indicates that this log message is from Discord.Net.
    /// </summary>
    Discord = 3,
    /// <summary>
    ///     Indicates that this log message is from the Discord HTTP REST API.
    /// </summary>
    Rest = 4,
    /// <summary>
    ///     Indicates that this log message is from the Discord WebSocket Gateway connection.
    /// </summary>
    Gateway = 5,
    /// <summary>
    ///     Indicates that this log message is from RyuBot itself.
    /// </summary>
    Bot = 6,
    /// <summary>
    ///     Indicates that this log message is from Sentry.
    /// </summary>
    Sentry = 7,
    /// <summary>
    ///     Indicates that this log message is from the Avalonia UI.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    UI = 8,
    /// <summary>
    ///     Indicates that this log message came from an unknown source.
    /// </summary>
    Unknown = int.MaxValue
}
using System.IO;
using System.Runtime.CompilerServices;
using Color = System.Drawing.Color;
using Optional = Gommon.Optional;

namespace Volte.Core.Helpers;

public static partial class Logger
{
    public static bool IsDebugLoggingEnabled => Config.EnableDebugLogging || Version.IsDevelopment;
    
    public static void HandleLogEvent(LogEventArgs args) =>
        Log(args.LogMessage.Severity, args.LogMessage.Source,
            args.LogMessage.Message, args.LogMessage.Exception, InvocationInfo.Here());

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Debug"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    public static void Debug(LogSource src, string message, Gommon.Optional<InvocationInfo> caller = default)
        => Log(LogSeverity.Debug, src, message, null, caller);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Info"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    public static void Info(LogSource src, string message, Gommon.Optional<InvocationInfo> caller = default)
        => Log(LogSeverity.Info, src, message, null, caller);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Error"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message, with the specified <paramref name="e"/> exception if provided.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    /// <param name="e">Optional Exception to print.</param>
    public static void Error(LogSource src, string message, Exception e = null, Gommon.Optional<InvocationInfo> caller = default)
        => Log(LogSeverity.Error, src, message, e, caller);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Critical"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message, with the specified <paramref name="e"/> exception if provided.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    /// <param name="e">Optional Exception to print.</param>
    public static void Critical(LogSource src, string message, Exception e = null, Gommon.Optional<InvocationInfo> caller = default)
        => Log(LogSeverity.Critical, src, message, e, caller);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Critical"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message, with the specified <paramref name="e"/> exception if provided.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    /// <param name="e">Optional Exception to print.</param>
    public static void Warn(LogSource src, string message, Exception e = null, Gommon.Optional<InvocationInfo> caller = default)
        => Log(LogSeverity.Warning, src, message, e, caller);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Verbose"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    public static void Verbose(LogSource src, string message, Gommon.Optional<InvocationInfo> caller = default)
        => Log(LogSeverity.Verbose, src, message, null, caller);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Error"/> message to the console from the specified <paramref name="e"/> exception.
    ///     This method calls <see cref="SentrySdk"/>'s CaptureException so it is logged to Sentry.
    /// </summary>
    /// <param name="e">Exception to print.</param>
    public static void Error(Exception e, Gommon.Optional<InvocationInfo> caller = default)
        => Execute(LogSeverity.Error, LogSource.Volte, string.Empty, e, caller);
}

public readonly struct InvocationInfo
{
    public required string SourceFileLocation { get; init; }
    public required int SourceFileLine { get; init; }
    public required string CallerName { get; init; }
    
    /// <summary>
    ///     Creates an <see cref="InvocationInfo"/> with information about the current source file, line, and method name.
    ///     Do not provide the arguments!
    /// </summary>
    /// <remarks>Mostly used in the logger.</remarks>
    /// <returns>An <see cref="InvocationInfo"/> referencing the specific line in the source file in which it is created.</returns>
    public static InvocationInfo Here(
        [CallerFilePath] string sourceLocation = default!,
        [CallerLineNumber] int lineNumber = default,
        [CallerMemberName] string callerName = default!) 
        => new()
    {
        SourceFileLocation = sourceLocation,
        SourceFileLine = lineNumber,
        CallerName = callerName
    };
    
    public bool IsInitialized => SourceFileLocation is not null && CallerName is not null;
    public string SourceFileName => SourceFileLocation[(SourceFileLocation.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
}
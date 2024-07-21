using System.IO;
using System.Runtime.CompilerServices;
using Color = System.Drawing.Color;
using Optional = Gommon.Optional;

namespace Volte.Helpers;

public static partial class Logger
{
    public static bool IsDebugLoggingEnabled => Config.DebugEnabled || Version.IsDevelopment;
    
    public static void HandleLogEvent(LogEventArgs args) =>
        Log<object>(args.LogMessage.Severity, args.LogMessage.Source,
            args.LogMessage.Message, args.LogMessage.Exception, default);

    #region Logger methods with invocation info

        /// <summary>
    ///     Prints a <see cref="LogSeverity.Debug"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    public static void Debug<TData>(LogSource src, string message, InvocationInfo<TData> caller)
        => Log(LogSeverity.Debug, src, message, null, caller);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Info"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    public static void Info<TData>(LogSource src, string message, InvocationInfo<TData> caller)
        => Log(LogSeverity.Info, src, message, null, caller);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Error"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message, with the specified <paramref name="e"/> exception if provided.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    /// <param name="e">Optional Exception to print.</param>
    public static void Error<TData>(LogSource src, string message, Exception e = null, InvocationInfo<TData> caller = default)
        => Log(LogSeverity.Error, src, message, e, caller);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Critical"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message, with the specified <paramref name="e"/> exception if provided.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    /// <param name="e">Optional Exception to print.</param>
    public static void Critical<TData>(LogSource src, string message, Exception e = null, InvocationInfo<TData> caller = default)
        => Log(LogSeverity.Critical, src, message, e, caller);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Critical"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message, with the specified <paramref name="e"/> exception if provided.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    /// <param name="e">Optional Exception to print.</param>
    public static void Warn<TData>(LogSource src, string message, Exception e = null, InvocationInfo<TData> caller = default)
        => Log(LogSeverity.Warning, src, message, e, caller);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Verbose"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    public static void Verbose<TData>(LogSource src, string message, InvocationInfo<TData> caller)
        => Log(LogSeverity.Verbose, src, message, null, caller);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Error"/> message to the console from the specified <paramref name="e"/> exception.
    ///     This method calls <see cref="SentrySdk"/>'s CaptureException, so it is logged to Sentry.
    /// </summary>
    /// <param name="e">Exception to print.</param>
    public static void Error<TData>(Exception e, InvocationInfo<TData> caller)
        => Execute(LogSeverity.Error, LogSource.Volte, string.Empty, e, caller);

    #endregion

    #region Normal logger methods
    
    /// <summary>
    ///     Prints a <see cref="LogSeverity.Debug"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    public static void Debug(LogSource src, string message)
        => Log<object>(LogSeverity.Debug, src, message, null);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Info"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    public static void Info(LogSource src, string message)
        => Log<object>(LogSeverity.Info, src, message, null);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Error"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message, with the specified <paramref name="e"/> exception if provided.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    /// <param name="e">Optional Exception to print.</param>
    public static void Error(LogSource src, string message, Exception e = null)
        => Log<object>(LogSeverity.Error, src, message, e);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Critical"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message, with the specified <paramref name="e"/> exception if provided.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    /// <param name="e">Optional Exception to print.</param>
    public static void Critical(LogSource src, string message, Exception e = null)
        => Log<object>(LogSeverity.Critical, src, message, e);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Critical"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message, with the specified <paramref name="e"/> exception if provided.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    /// <param name="e">Optional Exception to print.</param>
    public static void Warn(LogSource src, string message, Exception e = null)
        => Log<object>(LogSeverity.Warning, src, message, e);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Verbose"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message.
    /// </summary>
    /// <param name="src">Source to print the message from.</param>
    /// <param name="message">Message to print.</param>
    public static void Verbose(LogSource src, string message)
        => Log<object>(LogSeverity.Verbose, src, message, null);

    /// <summary>
    ///     Prints a <see cref="LogSeverity.Error"/> message to the console from the specified <paramref name="e"/> exception.
    ///     This method calls <see cref="SentrySdk"/>'s CaptureException, so it is logged to Sentry.
    /// </summary>
    /// <param name="e">Exception to print.</param>
    public static void Error(Exception e)
        => Execute<object>(LogSeverity.Error, LogSource.Volte, string.Empty, e, default);
    
    #endregion
}

public readonly struct FullDebugInfo
{
    public required SourceFileLocation SourceFileLocation { get; init; }
    public required string CallerName { get; init; }
}
    
public readonly struct SourceFileLocation
{
    public required string FilePath { get; init; }
    public required int LineInFile { get; init; }
}
    
public readonly struct SourceMemberName
{
    public required string Value { get; init; }
}

public readonly struct InvocationInfo<TData>
{
    // ReSharper disable once UnusedMember.Global
    // this is used by the default keyword
    public InvocationInfo()
    {
        IsInitialized = false;
    }
    
    public InvocationInfo(TData data)
    {
        IsInitialized = true;
        Data = data;
    }
    
    public TData Data { get; }
    public bool IsInitialized { get; }
}

public static class InvocationInfo
{
    public static string GetSourceFileName(this InvocationInfo<FullDebugInfo> fdiInvocation)
        => fdiInvocation.Data.SourceFileLocation.FilePath[
            (fdiInvocation.Data.SourceFileLocation.FilePath.LastIndexOf(Path.DirectorySeparatorChar) + 1)..
        ];
    
    public static string GetSourceFileName(this InvocationInfo<SourceFileLocation> sflInvocation)
        => sflInvocation.Data.FilePath[(sflInvocation.Data.FilePath.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
    
    /// <summary>
    ///     Creates an <see cref="InvocationInfo"/> with information about the current source file, line, and member name.
    ///     Do not provide the arguments!
    /// </summary>
    /// <remarks>Mostly used in the logger.</remarks>
    /// <returns>An <see cref="InvocationInfo"/> referencing the specific line in the specific member, in the source file in which it is created.</returns>
    public static InvocationInfo<FullDebugInfo> Here(
        [CallerFilePath] string sourceLocation = default!,
        [CallerLineNumber] int lineNumber = default,
        [CallerMemberName] string callerName = default!) 
        => new(new FullDebugInfo
        {
            SourceFileLocation = new SourceFileLocation
            {
                FilePath = sourceLocation,
                LineInFile = lineNumber
            },
            CallerName = callerName
        });
    
    /// <summary>
    ///     Creates a partial <see cref="InvocationInfo"/> with information about the current source file and line.
    ///     Do not provide the arguments!
    /// </summary>
    /// <remarks>Mostly used in the logger.</remarks>
    /// <returns>An <see cref="InvocationInfo"/> referencing the specific line in the source file in which it is created.</returns>
    public static InvocationInfo<SourceFileLocation> CurrentFileLocation(
        [CallerFilePath] string sourceLocation = default!,
        [CallerLineNumber] int lineNumber = default) 
        => new(new SourceFileLocation
        {
            FilePath = sourceLocation,
            LineInFile = lineNumber
        });
    
    /// <summary>
    ///     Creates a partial <see cref="InvocationInfo"/> with only information about the current member name.
    ///     Do not provide the arguments!
    /// </summary>
    /// <remarks>Mostly used in the logger.</remarks>
    /// <returns>An <see cref="InvocationInfo"/> referencing the specific C# source member it was created in.</returns>
    public static InvocationInfo<SourceMemberName> CurrentMember(
        [CallerMemberName] string callerName = default!) 
        => new(new SourceMemberName
        {
            Value = callerName
        });
}
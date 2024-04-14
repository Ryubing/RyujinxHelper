using System.IO;
using Colorful;
using Sentry.Extensibility;

using Color = System.Drawing.Color;

namespace Volte.Core.Helpers
{
    public static class Logger
    {
        public class SentryDiagnosticLogger : IDiagnosticLogger
        {
            public bool IsEnabled(SentryLevel logLevel) 
                => Version.IsDevelopment || logLevel is not SentryLevel.Debug;
            
            public void Log(SentryLevel logLevel, string message, Exception exception = null, params object[] args)
                => Logger.Lock.Lock(() =>
                {
                    var content = new StringBuilder();
                    var (color, value) = VerifySentryLevel(logLevel);
                    Append($"{value}:".PadRight(10), color);
                    var dt = DateTime.Now.ToLocalTime();
                    content.Append($"[{dt.FormatDate()} | {dt.FormatFullTime()}] {value} -> ");

                    (color, value) = VerifySource(LogSource.Sentry);
                    Append($"[{value}]".PadRight(10), color);
                    content.Append($"{value} -> ");

                    if (!message.IsNullOrWhitespace())
                        Append(message.Format(args), Color.White, ref content);

                    if (exception != null)
                    {
                        var toWrite = $"{Environment.NewLine}{exception.Message}{Environment.NewLine}{exception.StackTrace}";
                        Append(toWrite, Color.IndianRed, ref content);
                    }
                    
                    Console.Write(Environment.NewLine);

                    if (Environment.NewLine == content[^1].ToString()) return;
                    content.AppendLine();
                });
        }
        
        static Logger() => Directory.CreateDirectory("logs");
        
        public static readonly object Lock = new object();
        private const string LogFile = "logs/Volte.log";

        public static void HandleLogEvent(LogEventArgs args) =>
            Log(args.LogMessage.Severity, args.LogMessage.Source,
                args.LogMessage.Message, args.LogMessage.Exception);

        internal static void PrintHeader()
        {
            Info(LogSource.Volte, CommandsService.Separator.Trim());
            new Figlet().ToAscii("Volte").ConcreteValue.Split("\n", StringSplitOptions.RemoveEmptyEntries)
                .ForEach(ln => Info(LogSource.Volte, ln));
            Info(LogSource.Volte, CommandsService.Separator.Trim());
            Info(LogSource.Volte, $"Currently running Volte V{Version.FullVersion}.");
        }

        private static void Log(LogSeverity s, LogSource from, string message, Exception e = null)
        {
            if (s is LogSeverity.Debug && !Config.EnableDebugLogging)
                return;
            
            Lock.Lock(() => Execute(s, from, message, e));
        }

        /// <summary>
        ///     Prints a <see cref="LogSeverity.Debug"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message.
        /// </summary>
        /// <param name="src">Source to print the message from.</param>
        /// <param name="message">Message to print.</param>
        public static void Debug(LogSource src, string message)
            => Log(LogSeverity.Debug, src, message);

        /// <summary>
        ///     Prints a <see cref="LogSeverity.Info"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message.
        /// </summary>
        /// <param name="src">Source to print the message from.</param>
        /// <param name="message">Message to print.</param>
        public static void Info(LogSource src, string message)
            => Log(LogSeverity.Info, src, message);

        /// <summary>
        ///     Prints a <see cref="LogSeverity.Error"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message, with the specified <paramref name="e"/> exception if provided.
        /// </summary>
        /// <param name="src">Source to print the message from.</param>
        /// <param name="message">Message to print.</param>
        /// <param name="e">Optional Exception to print.</param>
        public static void Error(LogSource src, string message, Exception e = null)
            => Log(LogSeverity.Error, src, message, e);

        /// <summary>
        ///     Prints a <see cref="LogSeverity.Critical"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message, with the specified <paramref name="e"/> exception if provided.
        /// </summary>
        /// <param name="src">Source to print the message from.</param>
        /// <param name="message">Message to print.</param>
        /// <param name="e">Optional Exception to print.</param>
        public static void Critical(LogSource src, string message, Exception e = null)
            => Log(LogSeverity.Critical, src, message, e);

        /// <summary>
        ///     Prints a <see cref="LogSeverity.Critical"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message, with the specified <paramref name="e"/> exception if provided.
        /// </summary>
        /// <param name="src">Source to print the message from.</param>
        /// <param name="message">Message to print.</param>
        /// <param name="e">Optional Exception to print.</param>
        public static void Warn(LogSource src, string message, Exception e = null)
            => Log(LogSeverity.Warning, src, message, e);

        /// <summary>
        ///     Prints a <see cref="LogSeverity.Verbose"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message.
        /// </summary>
        /// <param name="src">Source to print the message from.</param>
        /// <param name="message">Message to print.</param>
        public static void Verbose(LogSource src, string message)
            => Log(LogSeverity.Verbose, src, message);

        /// <summary>
        ///     Prints a <see cref="LogSeverity.Error"/> message to the console from the specified <paramref name="e"/> exception.
        ///     This method calls <see cref="SentrySdk"/>'s CaptureException so it is logged to Sentry.
        /// </summary>
        /// <param name="e">Exception to print.</param>
        public static void Exception(Exception e)
            => Execute(LogSeverity.Error, LogSource.Volte, string.Empty, e);

        private static void Execute(LogSeverity s, LogSource src, string message, Exception e)
        {
            var content = new StringBuilder();
            var (color, value) = VerifySeverity(s);
            Append($"{value}:".PadRight(10), color);
            var dt = DateTime.Now.ToLocalTime();
            content.Append($"[{dt.FormatDate()} | {dt.FormatFullTime()}] {value} -> ");

            (color, value) = VerifySource(src);
            Append($"[{value}]".PadRight(10), color);
            content.Append($"{value} -> ");

            if (!message.IsNullOrWhitespace())
                Append(message, Color.White, ref content);

            if (e != null)
            {
                SentrySdk.CaptureException(e);
                Append(Environment.NewLine + e.Message + 
                       Environment.NewLine + e.StackTrace, 
                    Color.IndianRed, ref content);
            }

            Console.Write(Environment.NewLine);
            content.AppendLine();
            if (Config.EnabledFeatures?.LogToFile ?? false)
                File.AppendAllText(NormalizeLogFilePath(DateTime.Now), content.ToString());
        }

        private static string NormalizeLogFilePath(DateTime date) => LogFile.Replace("Volte", $"{date.Month}-{date.Day}-{date.Year}");

        private static void Append(string m, Color c)
        {
            Console.ForegroundColor = c;
            Console.Write(m);
        }

        private static void Append(string m, Color c, ref StringBuilder sb)
        {
            Console.ForegroundColor = c;
            Console.Write(m);
            sb.Append(m);
        }

        private static (Color Color, string Source) VerifySource(LogSource source) =>
            source switch
            {
                LogSource.Discord => (Color.RoyalBlue, "DISCORD"),
                LogSource.Gateway => (Color.RoyalBlue, "DISCORD"),
                LogSource.Volte => (Color.LawnGreen, "CORE"),
                LogSource.Service => (Color.Gold, "SERVICE"),
                LogSource.Module => (Color.LimeGreen, "MODULE"),
                LogSource.Rest => (Color.Red, "REST"),
                LogSource.Unknown => (Color.Fuchsia, "UNKNOWN"),
                LogSource.Sentry => (Color.Chartreuse, "SENTRY"),
                _ => throw new InvalidOperationException($"The specified LogSource {source} is invalid.")
            };
        
        private static (Color Color, string Level) VerifySentryLevel(SentryLevel level) =>
            level switch
            {
                SentryLevel.Debug => (Color.RoyalBlue, "DEBUG"),
                SentryLevel.Info => (Color.RoyalBlue, "INFO"),
                SentryLevel.Warning => (Color.LawnGreen, "WARN"),
                SentryLevel.Error => (Color.Gold, "ERROR"),
                SentryLevel.Fatal => (Color.LimeGreen, "FATAL"),
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
            };


        private static (Color Color, string Level) VerifySeverity(LogSeverity severity) =>
            severity switch
            {
                LogSeverity.Critical => (Color.Maroon, "CRITICAL"),
                LogSeverity.Error => (Color.DarkRed, "ERROR"),
                LogSeverity.Warning => (Color.Yellow, "WARN"),
                LogSeverity.Info => (Color.SpringGreen, "INFO"),
                LogSeverity.Verbose => (Color.Pink, "VERBOSE"),
                LogSeverity.Debug => (Color.SandyBrown, "DEBUG"),
                _ => throw new InvalidOperationException($"The specified LogSeverity ({severity}) is invalid.")
            };
    }
}
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volte.Core;
using Version = Volte.Version;

namespace Gommon
{
    public static partial class Extensions
    {
        public static IServiceCollection AddAllServices(this IServiceCollection coll) =>
            coll.AddVolteServices()
                .AddSingleton<CancellationTokenSource>()
                .AddSingleton(new HttpClient
                {
                    Timeout = 10.Seconds()
                })
                .AddSingleton(SentrySdk.Init(opts =>
                {
                    opts.Dsn = Config.SentryDsn;
                    opts.Debug = Config.EnableDebugLogging || Version.IsDevelopment;
                    opts.DiagnosticLogger = new Logger.SentryDiagnosticLogger();
                }))
                .AddSingleton(new CommandService(new CommandServiceConfiguration
                {
                    IgnoresExtraArguments = true,
                    StringComparison = StringComparison.OrdinalIgnoreCase,
                    DefaultRunMode = RunMode.Sequential,
                    SeparatorRequirement = SeparatorRequirement.SeparatorOrWhitespace,
                    Separator = " ",
                    NullableNouns = null
                }))
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = Severity,
                    GatewayIntents = Intents,
                    AlwaysDownloadUsers = true,
                    ConnectionTimeout = 10000,
                    MessageCacheSize = 50
                }));

        private static LogSeverity Severity => Version.IsDevelopment ? LogSeverity.Debug : LogSeverity.Verbose;

        private static GatewayIntents Intents
            => GatewayIntents.Guilds | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMembers |
               GatewayIntents.GuildMessages | GatewayIntents.GuildPresences | GatewayIntents.MessageContent;

        private static bool IsEligibleService(Type type) => type.Inherits<IVolteService>() && !type.IsAbstract;

        public static IServiceCollection AddVolteServices(this IServiceCollection serviceCollection)
            => serviceCollection.Apply(coll =>
            {
                //get all the classes that inherit IVolteService, and aren't abstract.
                var l = typeof(VolteBot).Assembly.GetTypes()
                    .Where(IsEligibleService).Apply(ls => ls.ForEach(coll.TryAddSingleton));
                Logger.Info(LogSource.Volte, $"Injected {l.Count()} services into the provider.");
            });
    }
}
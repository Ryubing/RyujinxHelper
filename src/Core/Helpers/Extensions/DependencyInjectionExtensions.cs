using Microsoft.Extensions.DependencyInjection.Extensions;
using Volte.Core;
using Version = Volte.Version;

namespace Gommon;

public static partial class Extensions
{
    public static IServiceCollection AddAllServices(this IServiceCollection coll) =>
        coll.AddSingleton<CancellationTokenSource>()
            .AddSingleton(new HttpClient
            {
                Timeout = 10.Seconds()
            })
            .AddSingleton(SentrySdk.Init(opts =>
            {
                opts.Dsn = Config.SentryDsn;
                opts.Debug = Config.EnableDebugLogging || Version.IsDevelopment;
                opts.DiagnosticLogger = new Logger.SentryTranslator();
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
                GatewayIntents = _intents,
                AlwaysDownloadUsers = true,
                ConnectionTimeout = 10000,
                MessageCacheSize = 50
            }))
            .Apply(_ =>
            {
                //get all the classes that inherit IVolteService or VolteExtension, and aren't abstract.
                var l = typeof(VolteBot).Assembly.GetTypes()
                    .Where(IsEligibleService)
                    .Apply(ls => ls.ForEach(coll.TryAddSingleton));
                Logger.Info(LogSource.Volte, $"Injected {l.Count()} services into the provider.");
            });

    private static LogSeverity Severity => Version.IsDevelopment ? LogSeverity.Debug : LogSeverity.Verbose;

    private static readonly GatewayIntents _intents
        = GatewayIntents.Guilds | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMembers |
           GatewayIntents.GuildMessages | GatewayIntents.GuildPresences | GatewayIntents.MessageContent;

    private static bool IsEligibleService(Type type) => type.Inherits<IVolteService>() && !type.IsAbstract;
}
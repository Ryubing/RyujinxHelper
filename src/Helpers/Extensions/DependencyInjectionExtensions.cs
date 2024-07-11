using Microsoft.Extensions.DependencyInjection.Extensions;
using Volte;
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
                opts.Debug = IsDebugLoggingEnabled;
                opts.DiagnosticLogger = new SentryTranslator();
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
                LogLevel = Version.IsDevelopment ? LogSeverity.Debug : LogSeverity.Verbose,
                GatewayIntents = _intents,
                AlwaysDownloadUsers = true,
                ConnectionTimeout = 10000,
                MessageCacheSize = 50
            }))
            .Apply(_ =>
            {
                //get all the classes that inherit IVolteService, and aren't abstract.
                var l = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(IsEligibleService)
                    .Apply(ls => ls.ForEach(coll.TryAddSingleton));
                Info(LogSource.Volte, $"Injected {l.Count()} services into the provider.");
            });

    private const GatewayIntents _intents
        = GatewayIntents.Guilds | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMembers |
           GatewayIntents.GuildMessages | GatewayIntents.GuildPresences | GatewayIntents.MessageContent;

    private static bool IsEligibleService(Type type) => type.Inherits<VolteService>() && !type.IsAbstract;
}
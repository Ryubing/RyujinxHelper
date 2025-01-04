using Microsoft.Extensions.DependencyInjection.Extensions;
using Octokit;
using RyuBot;
using Version = RyuBot.Version;

namespace Gommon;

public static partial class Extensions
{
    public static IServiceCollection AddAllServices(this IServiceCollection coll) =>
        coll.AddSingleton(new HttpClient
            {
                Timeout = 10.Seconds()
            })
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = Config.DebugEnabled || Version.IsDevelopment 
                    ? LogSeverity.Debug 
                    : LogSeverity.Verbose,
                GatewayIntents = GatewayIntents.None,
                ConnectionTimeout = 10000,
                MessageCacheSize = 0
            }))
            .Apply(_ =>
            {
                var ghc = new GitHubClient(new ProductHeaderValue("RyujinxHelper", Version.DotNetVersion.ToString()));
                var (username, password) = Config.GitHubLogin;
                ghc.Credentials = new Credentials(username, password);

                coll.AddSingleton(ghc);
                
                if (!Config.SentryDsn.IsNullOrEmpty())
                    coll.AddSingleton(SentrySdk.Init(opts =>
                    {
                        opts.Dsn = Config.SentryDsn;
                        opts.Debug = IsDebugLoggingEnabled;
                        opts.DiagnosticLogger = new SentryTranslator();
                    }));
                
                //get all the classes that inherit BotService, and aren't abstract; add them to the service provider
                var l = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(IsEligibleService)
                    .Apply(ls => ls.ForEach(coll.TryAddSingleton));
                Info(LogSource.Volte, $"Injected services [{l.Select(static x => x.Name.ReplaceIgnoreCase("Service", "")).JoinToString(", ")}] into the provider.");
            });

    private static bool IsEligibleService(Type type) => type.Inherits<BotService>() && !type.IsAbstract;
}
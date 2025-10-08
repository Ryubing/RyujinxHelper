using Qmmands;

namespace RyuBot;

public class RyujinxBot
{
    public static Task StartAsync(Gommon.Optional<CancellationTokenSource> cts = default)
    {
        Console.Title = $"RyuBot {Version.InformationVersion}";
        Console.CursorVisible = false;
        return RunAsync(cts);
    }

    public static bool IsHeadless { get; set; }

    public static ServiceProvider Services { get; private set; }

    public static DiscordSocketClient Client { get; private set; }
    public static CancellationTokenSource Cts { get; private set; }

    public RyujinxBot()
        => Console.CancelKeyPress += (_, _) => Cts?.Cancel();

    public static async Task RunAsync(Gommon.Optional<CancellationTokenSource> cts = default)
    {
        var sw = await LoginAsync(cts);
        if (sw is null)
            return;
        
        try
        {
            SetAppStatus($"Logged in, took {sw.Elapsed.Humanize(2)}.",
                FontAwesome.Check,
                isWorkingStatus: false,
                statusExpiresAfter: 10.Seconds()
            );
            await Task.Delay(-1, Cts.Token);
        }
        catch (Exception e)
        {
            e.SentryCapture();

            await ShutdownAsync();
        }
    }
    

    public static async Task<Stopwatch> LoginAsync(Gommon.Optional<CancellationTokenSource> cts = default)
    {
        if (!Config.StartupChecks<HeadlessBotConfig>()) 
            return null;

        Config.Load<HeadlessBotConfig>();

        if (!Config.IsValidToken()) 
            return null;

        LogFileRestartNotice();

        Services = new ServiceCollection().AddAllServices()
            .AddSingleton(cts.OrElse(new CancellationTokenSource()))
            .BuildServiceProvider();

        Client = Services.Get<DiscordSocketClient>();
        Cts = Services.Get<CancellationTokenSource>();

        SetAppStatus("Logging in", FontAwesome.RightToBracket);
        var sw = Stopwatch.StartNew();
        await Client.LoginAsync(TokenType.Bot, Config.Token);
        await Client.StartAsync();
        
        {
            var commandService = Services.Get<CommandService>();

            var addedParsers = MessageService.AddTypeParsers(commandService);
            Info(LogSource.Bot,
                $"Loaded TypeParsers: [{
                    addedParsers.Select(x => x.Name.Replace("Parser", string.Empty)).JoinToString(", ")
                }]");

            var addedModules = commandService.AddModules(Assembly.GetExecutingAssembly());
            Info(LogSource.Bot,
                $"Loaded {addedModules.Count} modules and {addedModules.Sum(m => m.Commands.Count)} commands.");
        }

        Client.RegisterEventHandlers(Services);
        
        Services.Get<CompatibilityCsvService>().Init();
        ExecuteBackgroundAsync(Services.Get<GitLabService>().InitAsync);
        
        sw.Stop();

        return sw;
    }
    
    public static async Task ShutdownAsync()
    {
        Critical(LogSource.Bot, "Bot shutdown requested; shutting down and cleaning up.");

        await Client.SetStatusAsync(UserStatus.Invisible);
        await Client.LogoutAsync();
        await Client.StopAsync();

        await Services.DisposeAsync();

        Services = null;
        Client = null;
        Cts = null;
    }
}
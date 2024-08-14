using Volte.Commands.Text.Modules;

namespace Volte;

public class VolteBot
{
    public static Task StartAsync(Gommon.Optional<CancellationTokenSource> cts = default)
    {
        Console.Title = $"Volte {Version.InformationVersion}";
        Console.CursorVisible = false;
        return new VolteBot().LoginAsync(cts);
    }

    public static bool AvaloniaIsAttached { get; set; }
    
    public static ServiceProvider Services { get; private set; }
    
    public static DiscordSocketClient Client { get; private set; }
    public static CancellationTokenSource Cts { get; private set; }

    public VolteBot()
        => Console.CancelKeyPress += (_, _) => Cts?.Cancel();

    public async Task LoginAsync(Gommon.Optional<CancellationTokenSource> cts = default)
    {
        if (!Config.StartupChecks()) return;

        Config.Load();

        if (!Config.IsValidToken()) return;

        LogFileRestartNotice();

        Services = new ServiceCollection().AddAllServices()
            .AddSingleton(cts.OrElse(new CancellationTokenSource()))
            .BuildServiceProvider();

        Client = Services.Get<DiscordSocketClient>();
        Cts = Services.Get<CancellationTokenSource>();

        AdminUtilityModule.AllowedPasteSites = await HttpHelper.GetAllowedPasteSitesAsync(Services);

        await Client.LoginAsync(TokenType.Bot, Config.Token);
        await Client.StartAsync();

        {
            var commandService = Services.Get<CommandService>();

            var addedParsers = commandService.AddTypeParsers();
            Info(LogSource.Volte,
                $"Loaded TypeParsers: [{
                    addedParsers.Select(x => x.Name.Replace("Parser", string.Empty)).JoinToString(", ")
                }]");

            var addedModules = commandService.AddModules(Assembly.GetExecutingAssembly());
            Info(LogSource.Volte,
                $"Loaded {addedModules.Count} modules and {addedModules.Sum(m => m.Commands.Count)} commands.");
        }

        Client.RegisterVolteEventHandlers(Services);

        ExecuteBackgroundAsync(async () => await Services.Get<AddonService>().InitAsync());
        Services.Get<ReminderService>().Initialize();

        try
        {
            await Task.Delay(-1, Cts.Token);
        }
        catch (Exception e)
        {
            e.SentryCapture();

            await ShutdownAsync(Client, Services);
        }
    }

    // ReSharper disable SuggestBaseTypeForParameter
    public static async Task ShutdownAsync(DiscordSocketClient client, ServiceProvider provider)
    {
        Critical(LogSource.Volte, "Bot shutdown requested; shutting down and cleaning up.");

        CalledCommandsInfo.UpdateSaved(provider.Get<MessageService>());

        await provider.DisposeAsync();

        await client.SetStatusAsync(UserStatus.Invisible);
        await client.LogoutAsync();
        await client.StopAsync();

        Environment.Exit(0);
    }
}
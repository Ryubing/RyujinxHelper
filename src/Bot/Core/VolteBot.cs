using Volte.Commands.Text.Modules;

namespace Volte;

public class VolteBot
{
    public static Task StartAsync()
    {
        Console.Title = $"Volte {Version.InformationVersion}";
        Console.CursorVisible = false;
        return new VolteBot().LoginAsync();
    }

    public static bool AvaloniaIsAttached { get; set; }
    
    public static ServiceProvider ServiceProvider { get; private set; }
    private DiscordSocketClient _client;
    private CancellationTokenSource _cts;

    private VolteBot()
        => Console.CancelKeyPress += (_, _) => _cts?.Cancel();

    private async Task LoginAsync()
    {
        if (!Config.StartupChecks()) return;

        Config.Load();

        if (!Config.IsValidToken()) return;

        LogFileRestartNotice();

        ServiceProvider = new ServiceCollection().AddAllServices().BuildServiceProvider();

        _client = ServiceProvider.Get<DiscordSocketClient>();
        _cts = ServiceProvider.Get<CancellationTokenSource>();

        AdminUtilityModule.AllowedPasteSites = await HttpHelper.GetAllowedPasteSitesAsync(ServiceProvider);

        await _client.LoginAsync(TokenType.Bot, Config.Token);
        await _client.StartAsync();

        {
            var commandService = ServiceProvider.Get<CommandService>();

            var addedParsers = commandService.AddTypeParsers();
            Info(LogSource.Volte,
                $"Loaded TypeParsers: [{
                    addedParsers.Select(x => x.Name.Replace("Parser", string.Empty)).JoinToString(", ")
                }]");

            var addedModules = commandService.AddModules(Assembly.GetExecutingAssembly());
            Info(LogSource.Volte,
                $"Loaded {addedModules.Count} modules and {addedModules.Sum(m => m.Commands.Count)} commands.");
        }

        _client.RegisterVolteEventHandlers(ServiceProvider);

        ExecuteBackgroundAsync(async () => await ServiceProvider.Get<AddonService>().InitAsync());
        ServiceProvider.Get<ReminderService>().Initialize();

        try
        {
            await Task.Delay(-1, _cts.Token);
        }
        catch (Exception e)
        {
            e.SentryCapture();

            await ShutdownAsync(_client, ServiceProvider);
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
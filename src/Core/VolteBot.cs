using Silk.NET.Maths;
using Silk.NET.Windowing;
using Volte.Commands.Text.Modules;
using Volte.UI;

namespace Volte;

public class VolteBot
{
    public static bool IsRunning { get; private set; }

    public static Task StartAsync()
    {
        Console.Title = DefaultWindowOptions.Title;
        Console.CursorVisible = false;
        return new VolteBot().LoginAsync();
    }

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

        IsRunning = true;

        if (Program.CommandLineArguments.TryGetValue("ui", out var sizeStr)
            && !UiManager.TryCreateUi(ServiceProvider,
                DefaultWindowOptions,
                sizeStr.TryParse<int>(out var fsz) ? fsz : 17,
                out var uiStartError)
           ) Error(LogSource.UI, $"Could not create UI: {uiStartError}");


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

        await _client.RegisterVolteEventHandlersAsync(ServiceProvider);

        ExecuteBackgroundAsync(async () => await ServiceProvider.Get<AddonService>().InitAsync());
        ServiceProvider.Get<ReminderService>().Initialize();

        try
        {
            await Task.Delay(-1, _cts.Token);
        }
        catch (Exception e)
        {
            IsRunning = false;
            e.SentryCapture();

            await ShutdownAsync(_client, ServiceProvider);
        }
    }

    // ReSharper disable SuggestBaseTypeForParameter
    public static async Task ShutdownAsync(DiscordSocketClient client, ServiceProvider provider)
    {
        Critical(LogSource.Volte, "Bot shutdown requested; shutting down and cleaning up.");

        var messageService = provider.Get<MessageService>();

        CalledCommandsInfo.UpdateSaved(messageService);

        await provider.DisposeAsync();

        await client.SetStatusAsync(UserStatus.Invisible);
        await client.LogoutAsync();
        await client.StopAsync();

        Environment.Exit(0);
    }

    #region UI

    // WindowOptions.Default with custom title and larger base window
    public static readonly WindowOptions DefaultWindowOptions = new(
        isVisible: true,
        position: new Vector2D<int>(50, 50),
        size: new Vector2D<int>(1600, 900),
        framesPerSecond: 0,
        updatesPerSecond: 0.0,
        api: GraphicsAPI.Default,
        title: $"Volte {Version.InformationVersion}",
        windowState: WindowState.Normal,
        windowBorder: WindowBorder.Resizable,
        isVSync: true,
        shouldSwapAutomatically: true,
        videoMode: VideoMode.Default
    );

    #endregion
}
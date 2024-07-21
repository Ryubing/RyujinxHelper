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
        Console.Title = WndOpt.Title;
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

        if (Program.CommandLineArguments.TryGetValue("ui", out var sizeStr))
        {
            TryCreateUi(ServiceProvider, 
                sizeStr.TryParse<int>(out var fsz) ? fsz : 17, 
                out _);
        }
            
        
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

    public static UiManager<VolteUiState> Ui { get; private set; }

    // WindowOptions.Default with custom title and larger base window
    private static readonly WindowOptions WndOpt = new(
        isVisible: true,
        position: new Vector2D<int>(50, 50),
        size: new Vector2D<int>(1600, 900),
        framesPerSecond: 1000,
        updatesPerSecond: 0.0,
        api: GraphicsAPI.Default,
        title: $"Volte {Version.InformationVersion}",
        windowState: WindowState.Normal,
        windowBorder: WindowBorder.Resizable,
#if DEBUG
        isVSync: false,
#else
        isVSync: true,
#endif
        shouldSwapAutomatically: true,
        videoMode: VideoMode.Default
    );

    public static bool TryCreateUi(IServiceProvider provider, int fontSize, out string error)
    {
        if (Ui is not null)
        {
            error = "UI is already open.";
            return false;
        }
        
        try
        {
            Ui = UiManager.Create(new VolteUiLayer(provider), WndOpt, fontSize);
            
            // declared as illegal code by the Silk God (Main thread isn't the controller of the Window)
            new Thread(() =>
            {
                Ui.Run(); //returns when UI is closed
                Ui.Dispose();
                Ui = null;
            }) { Name = "Volte UI Thread" }.Start();
        }
        catch (Exception e)
        {
            Error(LogSource.UI, "Could not create UI thread", e);
            error = $"Error opening UI: {e.Message}";
            return false;
        }
        
        error = null;

        return true;
    }

    #endregion
}
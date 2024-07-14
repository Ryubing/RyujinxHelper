using Silk.NET.Maths;
using Silk.NET.Windowing;
using Volte.UI;

namespace Volte;

public class VolteBot
{
    public static bool IsRunning { get; private set; } = false;
    
    public static Task StartAsync(Dictionary<string, string> commandLine)
    {
        Console.Title = $"Volte {Version.InformationVersion}";
        Console.CursorVisible = false;
        return new VolteBot().LoginAsync(commandLine);
    }

    private ServiceProvider _provider;
    private DiscordSocketClient _client;
    private CancellationTokenSource _cts;

    public static ImGuiManager<VolteImGuiState> ImGui { get; set; } = null;

    public static WindowOptions WndOpt = new(
        isVisible: true,
        position: new Vector2D<int>(50, 50),
        size: new Vector2D<int>(1280, 720),
        framesPerSecond: 0.0,
        updatesPerSecond: 0.0,
        api: GraphicsAPI.Default,
        title: $"Volte {Version.InformationVersion}",
        windowState: WindowState.Normal,
        windowBorder: WindowBorder.Resizable,
        isVSync: true,
        shouldSwapAutomatically: true,
        videoMode: VideoMode.Default
    );

    public static bool TryCreateUi(IServiceProvider provider, out string error)
    {
        if (ImGui is not null)
        {
            error = "UI is already open.";
            return false;
        }
        try
        {
            ImGui = new ImGuiManager<VolteImGuiState>(new VolteImGuiLayer(provider), WndOpt);
            
            // declared as illegal code by the Silk God (Main thread isn't the controller of the Window)
            new Thread(() =>
            {
                ImGui.Run();
                ImGui.Dispose();
                ImGui = null;
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

    private VolteBot()
        => Console.CancelKeyPress += (_, _) => _cts?.Cancel();

    private async Task LoginAsync(Dictionary<string, string> commandLine)
    {
        if (!Config.StartupChecks()) return;

        Config.Load();

        if (!Config.IsValidToken()) return;

        LogFileRestartNotice();

        _provider = new ServiceCollection().AddAllServices().BuildServiceProvider();

        IsRunning = true;
        
        if (commandLine.TryGetValue("ui", out _))
            TryCreateUi(_provider, out _);
        
        _client = _provider.Get<DiscordSocketClient>();
        _cts = _provider.Get<CancellationTokenSource>();
        
        AdminUtilityModule.AllowedPasteSites = await HttpHelper.GetAllowedPasteSitesAsync(_provider);

        await _client.LoginAsync(TokenType.Bot, Config.Token);
        await _client.StartAsync();

        {
            var commandService = _provider.Get<CommandService>();

            var (sw1, addedParsers) = 
                Timed(() => commandService.AddTypeParsers());
            Info(LogSource.Volte,
                $"Loaded TypeParsers: [{addedParsers.Select(x => x.Name.Replace("Parser", string.Empty)).JoinToString(", ")}] in {sw1.ElapsedMilliseconds}ms.");

            var (sw2, addedModules) =
                Timed(() => commandService.AddModules(Assembly.GetExecutingAssembly()));
            Info(LogSource.Volte,
                $"Loaded {addedModules.Count} modules and {addedModules.Sum(m => m.Commands.Count)} commands in {sw2.ElapsedMilliseconds}ms.");
        }

        await _client.RegisterVolteEventHandlersAsync(_provider);

        ExecuteBackgroundAsync(async () => await _provider.Get<AddonService>().InitAsync());
        _provider.Get<ReminderService>().Initialize();

        try
        {
            await Task.Delay(-1, _cts.Token);
        }
        catch (Exception e)
        {
            IsRunning = false;
            if (e is not TaskCanceledException && e is not OperationCanceledException)
                SentrySdk.CaptureException(e); //only capture ACTUAL errors to Sentry, Canceled exceptions get thrown when the CTS is cancelled
            
            await ShutdownAsync(_client, _provider);
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
}
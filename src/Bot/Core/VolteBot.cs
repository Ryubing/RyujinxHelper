using System.IO;
using ImGuiNET;
using Silk.NET.Maths;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Volte.Commands.Text.Modules;
using Volte.UI;
using Image = SixLabors.ImageSharp.Image;

namespace Volte;

public class VolteBot
{
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

        if (Program.CommandLineArguments.TryGetValue("ui", out var sizeStr))
            CreateUi(sizeStr);

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

    private static void CreateUi(string sizeStr)
    {
        var uiParams = GetUiParams(sizeStr.TryParse<int>(out var fsz) ? fsz : 17);

        if (UiManager.TryCreateUi(uiParams, out var uiStartError))
        {
            UiManager.AddView(new VolteUiView());
            UiManager.StartThread("Volte UI Thread");
        }
        else Error(LogSource.UI, $"Could not create UI: {uiStartError!.Message}");
    }

    private static readonly string[] UiFontResourceKeys = [ "Regular", "Bold", "BoldItalic", "Italic" ];
    
    public static UiManager.CreateParams GetUiParams(int fontSize)
    {
        unsafe
        {
            return new UiManager.CreateParams
            {
                WindowIcon = loadIcon(),
                WOptions = DefaultWindowOptions,
                Theme = Spectrum.Dark,
                OnConfigureIo = _ => 
                {
                    UiFontResourceKeys.ForEach(key =>
                    {
                        using var embeddedFont = Assembly.GetExecutingAssembly().GetManifestResourceStream(key);
                        if (embeddedFont != null)
                            UiManager.LoadFontFromStream(embeddedFont, key, fontSize);
                    });
                }
            };
        }

        Image<Rgba32> loadIcon()
        {
            using var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VolteIcon");
            return iconStream == null 
                ? null 
                : Image.Load<Rgba32>(iconStream);
        }
    }
}
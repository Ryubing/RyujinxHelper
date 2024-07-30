using System.IO;
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
            IsRunning = false;
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

        if (UiManager.TryCreateUi(uiParams,out var uiStartError))
        {
            UiManager.AddView(new VolteUiView(ServiceProvider));
            UiManager.StartThread(uiParams.ThreadName);
        }
        else Error(LogSource.UI, $"Could not create UI: {uiStartError!.Message}");
    }
    public static UiManager.CreateParams GetUiParams(int fontSize)
    {
        unsafe //Spectrum.Dark/Light are pointers
        {
            return new UiManager.CreateParams
            {
                OnConfigureIO = io =>
                {
                    var ttf = FilePath.Data / "UiFont.ttf";
                    if (!ttf.ExistsAsFile)
                    {
                        using var embeddedFont = Assembly.GetExecutingAssembly().GetManifestResourceStream("UIFont");
                        if (embeddedFont != null)
                        {
                            using var fs = ttf.OpenCreate();
                            embeddedFont.Seek(0, SeekOrigin.Begin);
                            embeddedFont.CopyTo(fs);
                        }
                    }

                    io.Fonts.AddFontFromFileTTF(ttf.ToString(), fontSize);
                },
                WindowIcon = getIcon(),
                WOptions = DefaultWindowOptions,
                Theme = Spectrum.Dark,
                ThreadName = "Volte UI Thread"
            };
        }

        Image<Rgba32> getIcon()
        {
            Stream iconStream;
            return (iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VolteIcon")) == null
                ? null
                : Image.Load<Rgba32>(iconStream);
        }
    }
}
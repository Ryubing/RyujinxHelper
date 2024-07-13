using Volte.UI;

namespace Volte;

public class VolteBot
{
    public static Task StartAsync(bool ui)
    {
        Console.Title = "Volte";
        Console.CursorVisible = false;
        return new VolteBot().LoginAsync(ui);
    }

    private ServiceProvider _provider;
    private DiscordSocketClient _client;
    private CancellationTokenSource _cts;

    public static ImGuiManager<VolteImGuiState> ImGui { get; private set; }

    private VolteBot()
        => Console.CancelKeyPress += (_, _) => _cts?.Cancel();

    private async Task LoginAsync(bool ui)
    {
        if (!Config.StartupChecks()) return;

        Config.Load();

        if (!Config.IsValidToken()) return;

        LogFileRestartNotice();

        _provider = new ServiceCollection().AddAllServices().BuildServiceProvider();
        
        try
        {
            if (ui)
            {
                ImGui = new ImGuiManager<VolteImGuiState>(new VolteImGuiLayer(_provider));
                new Thread(ImGui.Run) { Name = "Volte UI Thread" }.Start();

                ExecuteBackgroundAsync(async () =>
                {
                    while (true)
                    {
                        if (ImGui.Layer.TaskQueue.TryDequeue(out var task))
                            await task();
                    }
                });
            }
        }
        catch (Exception e)
        {
            Error(LogSource.UI, "Could not create UI thread", e);
        }
        
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
        var db = provider.Get<DatabaseService>();

        db.UpdateCalledCommandsInfo(messageService.SuccessfulCommandCalls, messageService.FailedCommandCalls);

        await provider.DisposeAsync();

        await client.SetStatusAsync(UserStatus.Invisible);
        await client.LogoutAsync();
        await client.StopAsync();
        Environment.Exit(0);
    }
}
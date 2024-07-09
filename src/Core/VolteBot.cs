namespace Volte.Core;

public class VolteBot
{
    public static Task StartAsync()
    {
        Console.Title = "Volte";
        Console.CursorVisible = false;
        return new VolteBot().LoginAsync();
    }

    private ServiceProvider _provider;
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

        _provider = new ServiceCollection().AddAllServices().BuildServiceProvider();
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

        Executor.ExecuteBackgroundAsync(async () => await _provider.Get<AddonService>().InitAsync());
        _provider.Get<ReminderService>().Initialize();

        try
        {
            await Task.Delay(-1, _cts.Token);
        }
        catch (Exception e)
        {
            SentrySdk.CaptureException(e);
            await ShutdownAsync(_client, _provider);
        }
    }

    // ReSharper disable SuggestBaseTypeForParameter
    public static async Task ShutdownAsync(DiscordSocketClient client, ServiceProvider provider)
    {
        Critical(LogSource.Volte, "Bot shutdown requested; shutting down and cleaning up.");

        var messageService = provider.Get<MessageService>();
        var db = provider.Get<DatabaseService>();

        db.SaveCalledCommandsInfo(
            db.GetCalledCommandsInfo().Apply(cci =>
            {
                cci.Successful += messageService.SuccessfulCommandCalls;
                cci.Failed += messageService.FailedCommandCalls;
            })
        );

        await provider.DisposeAsync();

        await client.SetStatusAsync(UserStatus.Invisible);
        await client.LogoutAsync();
        await client.StopAsync();
        Environment.Exit(0);
    }
}
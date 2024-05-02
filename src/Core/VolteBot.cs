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
        
        Logger.LogFileRestartNotice();

        _provider = new ServiceCollection().AddAllServices().BuildServiceProvider();
        _client = _provider.Get<DiscordSocketClient>();
        _cts = _provider.Get<CancellationTokenSource>();

        AdminUtilityModule.AllowedPasteSites = await HttpHelper.GetAllowedPasteSitesAsync(_provider);
            
        await _client.LoginAsync(TokenType.Bot, Config.Token);
        await _client.StartAsync();

        var commandService = _provider.Get<CommandService>();

        var sw = Stopwatch.StartNew();
        var l = commandService.AddTypeParsers();
        sw.Stop();
        Logger.Info(LogSource.Volte,
            $"Loaded TypeParsers: [{l.Select(x => x.Name.Replace("Parser", string.Empty)).JoinToString(", ")}] in {sw.ElapsedMilliseconds}ms.");
        sw = Stopwatch.StartNew();
        var loaded = commandService.AddModules(GetType().Assembly);
        sw.Stop();
        Logger.Info(LogSource.Volte,
            $"Loaded {loaded.Count} modules and {loaded.Sum(m => m.Commands.Count)} commands in {sw.ElapsedMilliseconds}ms.");
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
    public static async Task ShutdownAsync(DiscordSocketClient client, IServiceProvider provider)
    {
        Logger.Critical(LogSource.Volte,
            "Bot shutdown requested; shutting down and cleaning up.");

        await client.SetStatusAsync(UserStatus.Invisible);
        await client.LogoutAsync();
        await client.StopAsync();
        Environment.Exit(0);
    }
}
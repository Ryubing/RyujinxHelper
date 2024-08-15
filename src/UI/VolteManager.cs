using Gommon;

namespace Volte.UI;

public class VolteManager
{
    static VolteManager()
    {
        Console.Title = $"Volte {Version.InformationVersion}";
        Console.CursorVisible = false;
    }
    
    private static VolteBot? _bot;
    private static Task? _botTask;

    public static CancellationTokenSource Cts = new();

    public static void Start()
    {
        if (_bot is not null) return;
        
        _bot = new VolteBot();
        _botTask = Task.Run(async () => await _bot.LoginAsync(Cts));
    }

    public static void Stop()
    {
        if (_bot is null) return;
        
        Cts.Cancel();
        _bot = null;
        _botTask = null;
        
        Cts.TryReset();
    }
}
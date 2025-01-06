using RyuBot.Helpers;

namespace RyuBot.UI;

public class BotManager
{
    private static Task? _botTask;

    public static CancellationTokenSource? Cts { get; private set; }

    public static void Start()
    {
        if (RyujinxBot.Client is not null && Cts is not null) return;

        Cts = new();
        
        _botTask = Task.Run(async () => await RyujinxBot.LoginAsync(Cts), Cts.Token);
    }
    
    public static async Task<int> StartWait()
    {
        if (RyujinxBot.IsHeadless)
            Logger.OutputLogToStandardOut();
        
        Start();
        await _botTask!;
        return 0;
    }
    
    public static void Stop()
    {
        if (RyujinxBot.Client is null && Cts is null) return;
        
        Cts!.Cancel();
        _botTask = null;

        Cts = null;
    }
    
    public static string GetConnectionState()
        => RyujinxBot.Client is null
            ? "Disconnected"
            : Enum.GetName(RyujinxBot.Client.ConnectionState) ?? "Disconnected";
}
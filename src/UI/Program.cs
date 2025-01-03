using Avalonia;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using RyuBot.Helpers;
using RyuBot.UI.Avalonia;
using Logger = RyuBot.Helpers.Logger;

namespace RyuBot.UI;

public class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task<int> Main(string[] args)
    {
        if (!UnixHelper.TryParseNamedArguments(args, out var output) && output.Error is not InvalidOperationException)
            Logger.Error(output.Error);
        
        RyujinxBot.IsHeadless = args.Contains("--no-gui");

        if (RyujinxBot.IsHeadless) 
            return await BotManager.StartWait();
        
        BotManager.Start();

        IconProvider.Current.Register<FontAwesomeIconProvider>();
        
        return BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<RyujinxBotApp>()
            .UsePlatformDetect()
            .WithInterFont();
}
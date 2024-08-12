using Avalonia;
using Avalonia.Logging;
using Gommon;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Volte.Helpers;
using Logger = Volte.Helpers.Logger;

namespace Volte.UI;

public class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        if (!UnixHelper.TryParseNamedArguments(args, out var output) && output.Error is not InvalidOperationException)
            Logger.Error(output.Error);

        new Thread(() => VolteBot.StartAsync().GetAwaiter().GetResult()).Start();

        VolteBot.AvaloniaIsAttached = true;
        
        return BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .Apply(_ => IconProvider.Current.Register<FontAwesomeIconProvider>());
}
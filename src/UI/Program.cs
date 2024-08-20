using Avalonia;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Volte.Helpers;
using Volte.UI.Avalonia;
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
        VolteBot.AvaloniaIsAttached = true;
        
        if (!UnixHelper.TryParseNamedArguments(args, out var output) && output.Error is not InvalidOperationException)
            Logger.Error(output.Error);

        VolteManager.Start();

        IconProvider.Current.Register<FontAwesomeIconProvider>();
        
        return BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<VolteApp>()
            .UsePlatformDetect()
            .WithInterFont();
}
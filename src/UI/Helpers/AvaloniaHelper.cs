using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Gommon;

namespace Volte.UI.Helpers;

public class AvaloniaHelper
{
    public static string GetResource(string assetSubdir) 
        => $"avares://Volte.UI/Assets/{assetSubdir}";
    
    public static Uri GetResourceUri(string assetSubdir) => new(GetResource(assetSubdir));
    
    public static bool RequestAvaloniaShutdown(int exitCode = 0) 
        => DesktopLifetime?.TryShutdown(exitCode) ?? false;
    
    public static IClassicDesktopStyleApplicationLifetime? DesktopLifetime
        => Application.Current?.ApplicationLifetime?.Cast<IClassicDesktopStyleApplicationLifetime>();
}
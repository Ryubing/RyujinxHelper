using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Gommon;

namespace RyuBot.UI.Helpers;

public static class AvaloniaHelper
{
    public static string GetResource(string assetSubdir) 
        => $"avares://RyuBot.UI/Assets/{assetSubdir}";
    
    public static Uri GetResourceUri(string assetSubdir) => new(GetResource(assetSubdir));
    
    public static bool RequestAvaloniaShutdown(int exitCode = 0) 
        => DesktopLifetime?.TryShutdown(exitCode) ?? false;
    
    public static IClassicDesktopStyleApplicationLifetime? DesktopLifetime
        => Application.Current?.ApplicationLifetime?.Cast<IClassicDesktopStyleApplicationLifetime>();

    public static bool TryGetDesktop(out IClassicDesktopStyleApplicationLifetime desktopLifetime) 
        => (desktopLifetime = DesktopLifetime!) != null;

    public static T Context<T>(this StyledElement styledElement)
        => styledElement.DataContext.HardCast<T>();
}
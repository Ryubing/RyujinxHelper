using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Gommon;

namespace Volte.UI.Helpers;

// ReSharper disable once InconsistentNaming
public static class OS
{
    public static async Task CopyToClipboard(string content)
    {
        if (DesktopLifetime?.MainWindow?.Clipboard is { } clipboard)
            await clipboard.SetTextAsync(content);
    }

    private static IClassicDesktopStyleApplicationLifetime? DesktopLifetime
        => Application.Current?.ApplicationLifetime.Cast<IClassicDesktopStyleApplicationLifetime>();
}
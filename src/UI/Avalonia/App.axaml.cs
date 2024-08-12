using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using Volte.UI.Avalonia.Pages;
using Volte.UI.Helpers;

namespace Volte.UI;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var shellView = new UIShellView();
            desktop.MainWindow = shellView;
            
            PageManager.Shared.Register(Page.Logs, "Logs", new LogsView(), Symbol.AllApps, "Bot Logs", isDefault: false, isFooter: true);
        }
        

        base.OnFrameworkInitializationCompleted();
    }
}
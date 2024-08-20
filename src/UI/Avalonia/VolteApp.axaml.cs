using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Gommon;
using Humanizer;
using Volte.UI.Avalonia.Pages;
using Volte.UI.Helpers;

namespace Volte.UI.Avalonia;

public class VolteApp : Application
{
    private static WindowNotificationManager? _notificationManager;

    public static TopLevel? XamlRoot { get; private set; }

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

            XamlRoot = shellView;

            shellView.Loaded += (_, _) => _notificationManager = new(XamlRoot)
            {
                Position = NotificationPosition.BottomRight,
                MaxItems = 4,
                Margin = new(0, 0, 4, 30)
            };

            PageManager.Init();
        }

        VolteManager.Start();

        base.OnFrameworkInitializationCompleted();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static void Notify(
        string message,
        string title = "Notice",
        NotificationType type = NotificationType.Information,
        TimeSpan? expiration = null,
        Action? onClick = null
    ) => Dispatcher.UIThread.Invoke(() =>
        _notificationManager?.Show(new Notification(title, message, type, expiration, onClick))
    );

    public static void NotifyError<TException>(TException ex, Optional<TimeSpan> expiration = default, Action? onClick = null) where TException : Exception =>
        Notify(
            message: ex.Message,
            title: typeof(TException).AsPrettyString(),
            type: NotificationType.Error,
            expiration: expiration.OrElse(10.Seconds()),
            onClick: onClick ?? (() => PageManager.Shared.Focus(PageType.Logs))
        );
}
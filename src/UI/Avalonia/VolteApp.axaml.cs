using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
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

    public static readonly KeyGesture OpenDevTools = new(Key.F4, KeyModifiers.Control);

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (AvaloniaHelper.TryGetDesktop(out var desktop))
        {
            XamlRoot = desktop.MainWindow = new UIShellView();
            
            desktop.MainWindow.Loaded += (_, _) => _notificationManager = new(XamlRoot)
            {
                Position = NotificationPosition.BottomRight,
                MaxItems = 4,
                Margin = new(0, 0, 4, 30)
            };

            TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
            {
                NotifyError(eventArgs.Exception);
                eventArgs.SetObserved();
            };
            
            desktop.MainWindow.Closing += (_, _) =>
            {
                LogsViewModel.UnregisterHandler();
                VolteManager.Stop();
            };

            PageManager.Init();
        }

        VolteManager.Start();
    }

    public static void Notify(Notification notification)
        => Dispatcher.UIThread.Invoke(() => _notificationManager?.Show(notification));

    // ReSharper disable twice MemberCanBePrivate.Global
    public static void Notify(
        string message,
        string title = "Notice",
        NotificationType type = NotificationType.Information,
        Optional<TimeSpan> expiration = default,
        Action? onClick = null
    )
        => Notify(new Notification
        {
            Title = title,
            Message = message,
            Type = type,
            Expiration = expiration.OrElse(5.Seconds()),
            OnClick = onClick
        });

    public static void NotifyError<TException>(
        TException ex,
        Optional<TimeSpan> expiration = default,
        Action? onClick = null
    ) where TException : Exception
        => Notify(
            message: ex.Message,
            title: typeof(TException).AsPrettyString(),
            type: NotificationType.Error,
            expiration: expiration.OrElse(10.Seconds()),
            onClick: onClick ?? (() => PageManager.Shared.Focus(PageType.Logs))
        );
}
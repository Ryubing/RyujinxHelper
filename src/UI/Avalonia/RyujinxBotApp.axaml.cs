﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
#if DEBUG
using Avalonia.Diagnostics;
using Avalonia.Media;
#endif
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Gommon;
using Humanizer;
using MenuFactory;
using MenuFactory.Abstractions;
using RyuBot.Entities;
using RyuBot.UI.Avalonia.Pages;
using RyuBot.UI.Helpers;

namespace RyuBot.UI.Avalonia;

public class RyujinxBotApp : Application
{
    private static WindowNotificationManager? _notificationManager;

    public static TopLevel? XamlRoot { get; private set; }
    
    /// <summary>
    /// Application <see cref="IMenuFactory"/> (used for extending the main menu at runtime)
    /// </summary>
    public static IMenuFactory MenuFactory { get; private set; } = null!;
    

    public static readonly KeyGesture OpenDevTools = new(Key.F4, KeyModifiers.Control);

    public static IStorageProvider StorageProvider => Current!
        .ApplicationLifetime.HardCast<IClassicDesktopStyleApplicationLifetime>()
        .MainWindow!.StorageProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        NotificationEventArgs.NotificationSent += notif =>
        {
            switch (notif)
            {
                case CustomNotificationEventArgs custom:
                    Notify(custom.Message, custom.Title, 
                        custom.RawType.HardCast<NotificationType>(), 
                        custom.Expiration,
                        notif.Clicked, notif.Closed
                        );
                    break;
                case ErrorNotificationEventArgs custom:
                    NotifyError(custom.Error, custom.Expiration, notif.Clicked, notif.Closed);
                    break;
            }
        };
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (AvaloniaHelper.TryGetDesktop(out var desktop))
        {
            var shelLView = new UIShellView();

            XamlRoot = desktop.MainWindow = shelLView;
            
            desktop.MainWindow.Loaded += (_, _) => _notificationManager = new(XamlRoot)
            {
                Position = NotificationPosition.BottomRight,
                MaxItems = 4,
                Margin = new(0, 0, 4, 30)
            };

            MenuFactory = new AvaloniaMenuFactory(XamlRoot);
            MenuFactory.AddMenuGroup<ShellViewMenu>();
            shelLView.MainMenu.ItemsSource = MenuFactory.Items;
            
            desktop.MainWindow.Closing += (_, _) =>
            {
                LogsViewModel.UnregisterHandler();
                BotManager.Stop();
            };
            
            TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
            {
                NotifyError(eventArgs.Exception);
                eventArgs.SetObserved();
            };

            PageManager.Init();
            
#if DEBUG
            XamlRoot.AttachDevTools(new DevToolsOptions
            {
                Gesture = OpenDevTools,
                Size = new Size(800, 800),
                LaunchView = DevToolsViewKind.LogicalTree,
                FocusHighlighterBrush = Brushes.Crimson
            });
#endif
        }
    }

    public static void Notify(Notification notification)
        => Dispatcher.UIThread.Invoke(() => _notificationManager?.Show(notification));

    // ReSharper disable twice MemberCanBePrivate.Global
    public static void Notify(
        string message,
        string title = "Notice",
        NotificationType type = NotificationType.Information,
        Optional<TimeSpan> expiration = default,
        Action? onClick = null,
        Action? onClose = null
    )
        => Notify(new Notification
        {
            Title = title,
            Message = message,
            Type = type,
            Expiration = expiration.OrElse(5.Seconds()),
            OnClick = onClick,
            OnClose = onClose
        });

    public static void NotifyError<TException>(
        TException ex,
        Optional<TimeSpan> expiration = default,
        Action? onClick = null,
        Action? onClose = null
    ) where TException : Exception
        => Notify(new Notification 
        {
            Title = typeof(TException).AsPrettyString(),
            Message = ex.Message,
            Type = NotificationType.Error,
            Expiration = expiration.OrElse(10.Seconds()),
            OnClick = onClick ?? (() => PageManager.Shared.Focus(PageType.Logs)),
            OnClose = onClose
        });
}
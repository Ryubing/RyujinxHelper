using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Discord;
using Gommon;
using RyuBot.Entities;
using RyuBot.Helpers;
using RyuBot.UI.Helpers;

namespace RyuBot.UI.Avalonia.Pages;

public partial class LogsViewModel : ObservableObject
{
    private const byte MaxLogsInMemory = 200;

    private readonly Lock _logSync = new();

    public required LogsView? View { get; init; }

    public required int LogsClearAmount { get; init; }

    [ObservableProperty] private ObservableCollection<LogModel> _logs = [];

    [ObservableProperty] private LogModel? _selected;

    public LogsViewModel() => Logger.Event += Receive;

    ~LogsViewModel() => Logger.Event -= Receive;

    public static void UnregisterHandler()
    {
        Logger.Event -= PageManager.Shared.GetViewModel<LogsViewModel>().Receive;
    }

    private void Receive(LogEventArgs eventArgs)
    {
        if (eventArgs is
            {
                Source: LogSource.Sentry,
                Severity: LogSeverity.Debug,
                Message: not null,
                Message.Length: > 200
            }
           ) return; //sentry debug messages are huge and break the log view entirely.
        
        lock (_logSync)
        {
            IfNeededRemoveLast(LogsClearAmount);
            
            Logs.Add(new LogModel(eventArgs));
                
            Lambda.Try(() => Dispatcher.UIThread.Invoke(() => View?.Viewer?.ScrollToEnd()));

            if (eventArgs.Error is not { } err) return;

            RyujinxBotApp.NotifyError(err);
            err.SentryCapture(scope =>
                scope.AddBreadcrumb(
                    "This exception might not have been thrown, and may not be important; it is merely being logged.")
            );
        }
    }

    private void IfNeededRemoveLast(int amount)
    {
        if (Logs.Count < MaxLogsInMemory) return;

        Logs.Index()
            .OrderByDescending(static x => x.Item.Date)
            .TakeLast(amount)
            .ForEach(toRemove => Logs.RemoveAt(toRemove.Index));
    }
}
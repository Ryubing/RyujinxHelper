using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gommon;
using Volte.Entities;
using Volte.Helpers;
using Volte.UI.Helpers;

namespace Volte.UI.Avalonia.Pages;

public partial class LogsViewModel : ObservableObject
{
    internal readonly LogsView View;

    [ObservableProperty] 
    private ObservableCollection<VolteLog> _logs = [];

    [ObservableProperty] 
    private VolteLog? _selected;
    
    public LogsViewModel(LogsView view)
    {
        View = view;
        Logger.LogEvent += Receive;
    }
    
    [RelayCommand]
    private void Copy()
    {
        if (Selected is not null)
            Executor.ExecuteBackgroundAsync(() => OS.CopyToClipboard(Selected.String));
    }

    [RelayCommand]
    private void CopyMarkdown()
    {
        if (Selected is not null)
            Executor.ExecuteBackgroundAsync(() => OS.CopyToClipboard(Selected.Markdown));
    }


    ~LogsViewModel() => Logger.LogEvent -= Receive;
    
    public const byte MaxLogsInMemory = 100;
    

    private void Receive(VolteLogEventArgs eventArgs)
    {
        if (eventArgs.Message.IsNullOrEmpty() || eventArgs.Message.IsNullOrWhitespace())
        {
            eventArgs.Error?.SentryCapture(scope => 
                scope.AddBreadcrumb("This exception might not have been thrown, and may not be important; it is merely being logged.")
            );
            return;
        }
        
        if (Logs.Count >= MaxLogsInMemory)
            Logs.OrderByDescending(x => x.Date)
                .FindFirst()
                .IfPresent(toRemove => Logs.Remove(toRemove));
        
        Logs.Add(new VolteLog(eventArgs));
        Dispatcher.UIThread.Invoke(View.Viewer.ScrollToEnd);
    }
}
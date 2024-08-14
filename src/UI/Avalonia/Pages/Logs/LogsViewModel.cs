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
    private const byte MaxLogsInMemory = 100;
    
    private readonly LogsView _view;

    [ObservableProperty] 
    private ObservableCollection<VolteLog> _logs = [];

    [ObservableProperty] 
    private VolteLog? _selected;
    
    public LogsViewModel(LogsView view)
    {
        _view = view;
        Logger.LogEvent += Receive;
    }
    
    ~LogsViewModel() => Logger.LogEvent -= Receive;
    
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
    

    private void Receive(VolteLogEventArgs eventArgs)
    {
        if (!(eventArgs.Message.IsNullOrEmpty() || eventArgs.Message.IsNullOrWhitespace()))
        {
            if (Logs.Count >= MaxLogsInMemory)
                Logs.OrderByDescending(x => x.Date)
                    .FindFirst()
                    .IfPresent(toRemove => Logs.Remove(toRemove));
        
            Logs.Add(new VolteLog(eventArgs));
            Lambda.Try(() => Dispatcher.UIThread.Invoke(_view.Viewer.ScrollToEnd));
        }
        
        eventArgs.Error?.SentryCapture(scope => 
            scope.AddBreadcrumb("This exception might not have been thrown, and may not be important; it is merely being logged.")
        );
    }
}
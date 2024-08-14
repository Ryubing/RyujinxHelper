using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Volte.UI.Helpers;

namespace Volte.UI.Avalonia.Pages;

[UiPage(PageType.Logs, 0, "Bot Logs", Symbol.AllApps, isFooter: true)]
public partial class LogsView : UserControl
{
    public LogsView()
    {
        InitializeComponent();
        DataContext = new LogsViewModel(this);
    }
}
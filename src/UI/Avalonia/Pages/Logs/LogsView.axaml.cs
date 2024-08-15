using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Volte.UI.Helpers;

namespace Volte.UI.Avalonia.Pages;

[UiPage(PageType.Logs, "Bot Logs", Symbol.AllApps, isFooter: true)]
public partial class LogsView : UserControl
{
    public LogsView()
    {
        InitializeComponent();
        DataContext = new LogsViewModel
        {
            View = this
        };
    }
}
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Gommon;
using Volte.Helpers;
using Volte.UI.Helpers;

namespace Volte.UI.Avalonia.Pages;

[UiPage(PageType.Logs, "Bot Logs", Symbol.AllApps, isFooter: true)]
public partial class LogsView : UserControl
{
    public LogsView()
    {
        InitializeComponent();
        DataContext = new LogsViewModel { View = this };

        CopySimpleIcon.Value = FontAwesome.Copy;
        CopySimple.Command = new AsyncRelayCommand(async () =>
        {
            if (DataContext.Cast<LogsViewModel>().Selected is { } selected)
                await OS.CopyToClipboardAsync(selected.String);
        });

        CopyMarkdownIcon.Value = FontAwesome.Brush;
        CopyMarkdown.Command = new AsyncRelayCommand(async () =>
        {
            if (DataContext.Cast<LogsViewModel>().Selected is { } selected)
                await OS.CopyToClipboardAsync(selected.Markdown);
        });
    }
}
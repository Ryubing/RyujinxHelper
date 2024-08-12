using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Volte.UI.Avalonia.Pages;

public partial class LogsView : UserControl
{
    public LogsView()
    {
        InitializeComponent();
        DataContext = new LogsViewModel(this);
    }
}
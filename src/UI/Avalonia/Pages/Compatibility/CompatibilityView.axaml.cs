using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using RyuBot.UI.Helpers;

namespace RyuBot.UI.Avalonia.Pages;

[UiPage(PageType.Compatibility, "Game Compatibility", Symbol.Games)]
public partial class CompatibilityView : UserControl
{
    public CompatibilityView()
    {
        InitializeComponent();
    }

    private void TextBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (DataContext is not CompatibilityViewModel cvm)
            return;

        if (sender is not TextBox searchBox)
            return;
        
        cvm.Search(searchBox.Text);
    }
}
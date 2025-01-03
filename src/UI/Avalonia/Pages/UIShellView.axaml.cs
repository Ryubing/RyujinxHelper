using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Windowing;
using RyuBot.UI.Helpers;

// ReSharper disable InconsistentNaming

namespace RyuBot.UI.Avalonia.Pages;

public partial class UIShellView : AppWindow
{
    public UIShellView()
    {
        InitializeComponent();
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        using var bitmap = new Bitmap(AssetLoader.Open(AvaloniaHelper.GetResourceUri("icon.ico")));
        Logo.Source = Icon = bitmap.CreateScaledBitmap(new PixelSize(48, 48));
        
        DataContext = new UIShellViewModel { View = this };
        
        PageManager.Shared.PropertyChanged += (pm, e) =>
        {
            if (e.PropertyName == nameof(PageManager.Current) && pm is PageManager pageManager)
                Navigation.Content = pageManager.Current?.Content;
        };
    }
}
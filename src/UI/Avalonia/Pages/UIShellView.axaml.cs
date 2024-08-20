using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Windowing;
using Gommon;
using Volte.UI.Helpers;
// ReSharper disable InconsistentNaming

namespace Volte.UI.Avalonia.Pages;

public partial class UIShellView : AppWindow
{
    public UIShellView()
    {
        InitializeComponent();
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        using var bitmap = new Bitmap(AssetLoader.Open(AvaloniaHelper.GetResourceUri("icon.ico")));
        VolteLogo.Source = Icon = bitmap.CreateScaledBitmap(new PixelSize(48, 48));
        
        DataContext = new UIShellViewModel
        {
            View = this,
            OpenDevTools = new KeyGesture(Key.F4, KeyModifiers.Control)
        };
        
        PageManager.Shared.PropertyChanged += (pm, e) =>
        {
            if (e.PropertyName == nameof(PageManager.Current) && pm is PageManager pageManager)
                Navigation.Content = pageManager.Current?.Content;
        };

#if DEBUG
        this.AttachDevTools(DataContext.Cast<UIShellViewModel>().OpenDevTools);
#endif
    }
}
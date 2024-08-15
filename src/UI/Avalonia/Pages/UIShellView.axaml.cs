using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Windowing;
using Gommon;
using Volte.UI.Helpers;

namespace Volte.UI.Avalonia.Pages;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public partial class UIShellView : AppWindow
{
    public UIShellView()
    {
        InitializeComponent();
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        using var bitmap = new Bitmap(AssetLoader.Open(AvaloniaHelper.GetResourceUri("icon.ico")));
        Icon = bitmap.CreateScaledBitmap(new PixelSize(48, 48));
        
        DataContext = new UIShellViewModel
        {
            ShellView = this,
            OpenDevTools = new KeyGesture(Key.F4, KeyModifiers.Control)
        };
        
        PageManager.Shared.PropertyChanged += (pm, e) =>
        {
            if (e.PropertyName == nameof(PageManager.Current) && pm is PageManager pageManager)
                SideNav.Content = pageManager.Current?.Content;
        };

#if DEBUG
        this.AttachDevTools(DataContext.Cast<UIShellViewModel>().OpenDevTools);
#endif
    }
}
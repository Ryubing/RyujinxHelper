using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Windowing;
using Gommon;

namespace Volte.UI;

public partial class UIShellView : AppWindow
{
    public UIShellView()
    {
        InitializeComponent();
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        
        using var bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://Volte.UI/Assets/icon.ico")));
        Icon = bitmap.CreateScaledBitmap(new PixelSize(48, 48));
        
        DataContext = new UIShellViewModel
        {
            OpenDevTools = new KeyGesture(Key.F4, KeyModifiers.Control),
            Icon = Icon
        };

#if DEBUG
        this.AttachDevTools(DataContext.Cast<UIShellViewModel>().OpenDevTools);
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
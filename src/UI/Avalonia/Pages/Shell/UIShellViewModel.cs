using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Volte.UI;

// ReSharper disable once InconsistentNaming
public partial class UIShellViewModel : ObservableObject
{
    public KeyGesture OpenDevTools { get; init; }
    
    public IImage Icon { get; init; }

    [ObservableProperty]
    private string _title = "Volte";

    public UIShellViewModel()
    {
        
    }
}
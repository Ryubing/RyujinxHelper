using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Discord;

namespace Volte.UI.Avalonia.Pages;

// ReSharper disable once InconsistentNaming
public partial class UIShellViewModel : ObservableObject
{
    public required UIShellView? View { get; init; } 
    
    public required KeyGesture OpenDevTools { get; init; }

    [ObservableProperty]
    private ConnectionState _connection = VolteBot.Client.ConnectionState;

    [ObservableProperty]
    private string _title = "Volte";

    public UIShellViewModel()
    {
        VolteBot.Client.Connected += ConnectionChanged;
        VolteBot.Client.Disconnected += _ => ConnectionChanged();
    }

    private Task ConnectionChanged()
    {
        Connection = VolteBot.Client.ConnectionState;
        return Task.CompletedTask;
    }
}
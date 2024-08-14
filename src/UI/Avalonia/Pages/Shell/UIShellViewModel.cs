using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Discord;
using Discord.WebSocket;

namespace Volte.UI;

// ReSharper disable once InconsistentNaming
public partial class UIShellViewModel : ObservableObject
{
    public KeyGesture OpenDevTools { get; init; }
    
    public IImage Icon { get; init; }

    [ObservableProperty]
    public ConnectionState _connection = VolteBot.Client.ConnectionState;

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
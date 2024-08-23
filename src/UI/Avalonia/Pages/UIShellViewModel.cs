using CommunityToolkit.Mvvm.ComponentModel;

namespace Volte.UI.Avalonia.Pages;

// ReSharper disable once InconsistentNaming
public partial class UIShellViewModel : ObservableObject
{
    public required UIShellView? View { get; init; } 

    [ObservableProperty]
    private string _connection = "Disconnected";

    public UIShellViewModel()
    {
        VolteBot.Client.Connected += ConnectionChanged;
        VolteBot.Client.Disconnected += _ => ConnectionChanged();
    }

    private Task ConnectionChanged()
    {
        Connection = VolteManager.GetConnectionState();
        return Task.CompletedTask;
    }
}
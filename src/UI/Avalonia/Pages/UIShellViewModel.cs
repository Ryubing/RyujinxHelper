using CommunityToolkit.Mvvm.ComponentModel;

namespace RyuBot.UI.Avalonia.Pages;

// ReSharper disable once InconsistentNaming
public partial class UIShellViewModel : ObservableObject
{
    public required UIShellView View { get; init; } 

    [ObservableProperty]
    private string _connection = "Disconnected";

    public string StateString => $"{Connection} | {View.Title} | {Version.InformationVersion}";

    public UIShellViewModel()
    {
        RyujinxBot.Client.Connected += ChangeConnectionState;
        RyujinxBot.Client.Disconnected += Disconnected;
    }

    ~UIShellViewModel()
    {
        RyujinxBot.Client.Connected -= ChangeConnectionState;
        RyujinxBot.Client.Disconnected -= Disconnected;
    }

    private Task ChangeConnectionState()
    {
        Connection = BotManager.GetConnectionState();
        OnPropertyChanged(nameof(StateString));
        return Task.CompletedTask;
    }
    
    private Task Disconnected(Exception e)
    {
        RyujinxBotApp.NotifyError(e);
        return ChangeConnectionState();
    }
}
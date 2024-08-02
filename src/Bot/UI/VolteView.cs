using ImGuiNET;
using Color = System.Drawing.Color;
// ReSharper disable InvertIf

namespace Volte.UI;

public sealed class VolteUiState
{
    public bool SelectedTheme = true;
    public bool ShowStyleEditor;
    
    public VolteUiState(IServiceProvider provider)
    {
        Cts = provider.Get<CancellationTokenSource>();
        Client = provider.Get<DiscordSocketClient>();
        Messages = provider.Get<MessageService>();
        Database = provider.Get<DatabaseService>();
    }

    public readonly CancellationTokenSource Cts;
    public readonly DiscordSocketClient Client;
    public readonly MessageService Messages;
    public readonly DatabaseService Database;

    public ulong SelectedGuildId = 0;
}

public partial class VolteUiView : UiView
{
    private readonly VolteUiState _state;
    
    public VolteUiView()
    {
        _state = new VolteUiState(VolteBot.ServiceProvider);
        
        MainMenuBar = MenuBar;
        
        Panel("Command Stats", CommandStats);
        Panel("Bot Management", BotManagement);
        Panel("Guild Manager", GuildManager);
        Panel(_ =>
        {
            if (_state.ShowStyleEditor)
            {
                ImGui.Begin("Style Editor");
                ImGui.ShowStyleEditor(ImGui.GetStyle());
                ImGui.End();
            }
        });
    }
    
    private void MenuBar(double delta)
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.Button("Shutdown"))
                _state.Cts.Cancel();
            
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Theming"))
        {
            if (ImGui.MenuItem(_state.SelectedTheme ? "Swap to Light" : "Swap to Dark"))
            {
                _state.SelectedTheme = !_state.SelectedTheme;
                unsafe
                {
                    UiManager.SetColors(_state.SelectedTheme ? Spectrum.Dark : Spectrum.Light);
                }

            }

            if (ImGui.RadioButton("Show Style Editor", _state.ShowStyleEditor))
                _state.ShowStyleEditor = !_state.ShowStyleEditor;
            
            ImGui.EndMenu();
        }
        
        ImGui.BeginMenu($"{Io.Framerate:###} FPS", false);
    }

    private static void ColoredText(string fmt, Color color) =>
        ImGui.TextColored(color.AsVec4(), fmt);
}
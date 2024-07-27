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

public partial class VolteUiLayer : UiLayer
{
    private readonly VolteUiState _state;
    
    public VolteUiLayer(IServiceProvider provider)
    {
        _state = new VolteUiState(provider);
        
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
        
        if (ImGui.BeginMenu("Debug Stats"))
        {
            ImGui.MenuItem($"{Io.Framerate:###} FPS ({1000f / Io.Framerate:0.##} ms/frame)", false);
            
            if (Config.DebugEnabled || Version.IsDevelopment)
            {
                ImGui.MenuItem($"Delta: {delta:0.00000}", false);
            }
            
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Theming"))
        {
            if (ImGui.MenuItem(_state.SelectedTheme ? "Swap to Light" : "Swap to Dark"))
            {
                _state.SelectedTheme = !_state.SelectedTheme;
                if (_state.SelectedTheme) 
                    SetColors(ref Spectrum.Dark, true);
                else SetColors(ref Spectrum.Light, false);
            }

            if (ImGui.RadioButton("Show Style Editor", _state.ShowStyleEditor))
                _state.ShowStyleEditor = !_state.ShowStyleEditor;
            
            ImGui.EndMenu();
        }
    }

    private static void ColoredText(string fmt, Color color) =>
        ImGui.TextColored(color.AsVec4(), fmt);
}
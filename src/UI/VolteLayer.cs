
using System.IO;
using ImGuiNET;
using Silk.NET.OpenGL.Extensions.ImGui;
using Color = System.Drawing.Color;

namespace Volte.UI;

public sealed class VolteUiState : UiLayerState
{
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

    public bool ShowStyleEditor = false;
}

public partial class VolteUiLayer : UiLayer<VolteUiState>
{
    public VolteUiLayer(IServiceProvider provider)
    {
        State = new VolteUiState(provider);
        
        MainMenuBar = MenuBar;
        
        Panel("UI Settings", UiSettings);
        Panel("Command Stats", CommandStats);
        Panel("Bot Management", BotManagement);
        Panel("Guild Manager", GuildManager);
        Panel(_ =>
        {
            if (State.ShowStyleEditor)
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
                State.Cts.Cancel();
            
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
    }

    public override ImGuiFontConfig? GetFontConfig(int size)
    {
        var ttf = FilePath.Data / "UiFont.ttf";
        if (!ttf.ExistsAsFile)
        {
            using var embeddedFont = Assembly.GetExecutingAssembly().GetManifestResourceStream("UIFont");
            if (embeddedFont != null)
            {
                using var fs = ttf.OpenCreate();
                embeddedFont.Seek(0, SeekOrigin.Begin);
                embeddedFont.CopyTo(fs);
            }
        }
        
        return new ImGuiFontConfig(ttf.ToString(), size);
    }

    private static void ColoredText(string fmt, Color color) =>
        ImGui.TextColored(color.AsVec4(), fmt);
}
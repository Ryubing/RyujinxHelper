using System.Collections.Immutable;
using ImGuiNET;

namespace Volte.UI;

public class VolteImGuiLayer : ImGuiLayer<VolteImGuiLayer.VolteImGuiState>
{
    public VolteImGuiLayer(IServiceProvider provider)
    {
        State = new VolteImGuiState(provider);
    }

    public sealed class VolteImGuiState : ImGuiLayerState
    {
        public VolteImGuiState(IServiceProvider provider)
        {
            Cts = provider.Get<CancellationTokenSource>();
            Client = provider.Get<DiscordSocketClient>();
        }

        public CancellationTokenSource Cts { get; }
        public DiscordSocketClient Client { get; }
    }
    
    public override void Render(double delta)
    {
        {
            ImGui.BeginMainMenuBar();
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.Button("Shutdown"))
                    State.Cts.Cancel();
                
                ImGui.EndMenu();
            }
            
            ImGui.EndMainMenuBar();
        }
        
        {
            ImGui.Begin("UI Settings");
            
            if (ImGui.Button("Reset Background Color"))
                State.Background = ImGuiLayerState.DefaultBackground;
            ImGui.ColorPicker3("Background", ref State.Background, ImGuiColorEditFlags.NoSidePreview);
            
            ImGui.End();
        }

        /*{
            ImGui.Begin("Bot Management");
            if (ImGui.BeginMenu("Guilds"))
            {
                State.Client.Guilds.ForEach(guild =>
                {
                    //ImGui.Menu
                });
            }
            ImGui.End();
        }*/

        if (Config.EnableDebugLogging || Version.IsDevelopment)
        {
            ImGui.Begin("Debug Info");
            ImGui.Text($"Delta: {delta}");
            var framerate = ImGui.GetIO().Framerate;
            ImGui.Text($"{framerate:###} FPS ({1000f / framerate:0.##} ms/frame)");
            ImGui.End();
        }
    }
}
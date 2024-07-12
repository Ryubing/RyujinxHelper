using ImGuiNET;

namespace Volte.UI;

public class VolteImGuiLayer : ImGuiLayer<VolteImGuiLayer.VolteImGuiState>
{
    public VolteImGuiLayer()
    {
        State = new VolteImGuiState();
    }

    public sealed class VolteImGuiState : ImGuiLayerState;
    
    public override void Render(double delta)
    {
        {
            ImGui.Begin("UI Settings");
            ImGui.ColorPicker3("Background", ref State.Background);
            if (ImGui.Button("Reset Background"))
                State.Background = ImGuiLayerState.DefaultBackground;
            
            ImGui.End();
        }
    }
}
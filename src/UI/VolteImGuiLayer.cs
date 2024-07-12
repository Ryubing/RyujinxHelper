using ImGuiNET;

namespace Volte.UI;

public class VolteImGuiLayer : ImGuiLayer
{
    public override void Render(double delta)
    {
        ImGui.Text("Hello");
    }
}
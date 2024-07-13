using ImGuiNET;
using Color = System.Drawing.Color;

namespace Volte.UI;

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

public class VolteImGuiLayer : ImGuiLayer<VolteImGuiState>
{
    public VolteImGuiLayer(IServiceProvider provider)
    {
        State = new VolteImGuiState(provider);
    }
    
    public override void Render(double delta)
    {
        {
            if (ImGui.BeginMainMenuBar())
            {
                MenuBar();
                ImGui.EndMainMenuBar();
            }

        }
        
        {
            ImGui.Begin("UI Settings");
            UiSettings();
            ImGui.End();
        }

        {
            ImGui.Begin("Bot Management");
            BotManagement();
            ImGui.End();
        }

        if (Config.EnableDebugLogging || Version.IsDevelopment)
        {
            ImGui.Begin("Debug Info");
            DebugPanel(delta);
            ImGui.End();
        }
    }
    
    public void MenuBar()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.Button("Shutdown"))
                State.Cts.Cancel();
                
            ImGui.EndMenu();
        }
    }

    public void UiSettings()
    {
        if (ImGui.Button("Reset Background Color"))
            State.Background = ImGuiLayerState.DefaultBackground;
        ImGui.ColorPicker3("Background", ref State.Background, ImGuiColorEditFlags.NoSidePreview);
    }

    public void BotManagement()
    {
        ImGui.Text("Discord status:");
        switch (State.Client.ConnectionState)
        {
            case ConnectionState.Connected:
                ImGui.TextColored(Color.LawnGreen.AsVec4(), "  Connected!");
                break;
            case ConnectionState.Connecting:
                ImGui.TextColored(Color.Yellow.AsVec4(), "  Connecting...");
                break;
            case ConnectionState.Disconnected:
                ImGui.TextColored(Color.Red.AsVec4(), "  Disconnected!");
                break;
            case ConnectionState.Disconnecting:
                ImGui.TextColored(Color.OrangeRed.AsVec4(), "  Disconnecting...");
                break;
        }

        ImGui.Text("Bot status:");
        if (ImGui.BeginMenu($"  {State.Client.Status}"))
        {
            if (ImGui.MenuItem("Online"))
                TaskQueue.Enqueue(() => State.Client.SetStatusAsync(UserStatus.Online));
            if (ImGui.MenuItem("Idle"))
                TaskQueue.Enqueue(() => State.Client.SetStatusAsync(UserStatus.Idle));
            if (ImGui.MenuItem("Do Not Disturb"))
                TaskQueue.Enqueue(() => State.Client.SetStatusAsync(UserStatus.DoNotDisturb));
            if (ImGui.MenuItem("Invisible"))
                TaskQueue.Enqueue(() => State.Client.SetStatusAsync(UserStatus.Invisible));
            ImGui.EndMenu();
        }
    }

    public void DebugPanel(double delta)
    {
        ImGui.Text($"Delta: {delta}");
        var framerate = ImGui.GetIO().Framerate;
        ImGui.Text($"{framerate:###} FPS ({1000f / framerate:0.##} ms/frame)");
    }
}
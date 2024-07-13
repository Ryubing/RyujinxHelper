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
                MenuBar(delta);
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
    }
    
    public void MenuBar(double delta)
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.Button("Shutdown"))
                State.Cts.Cancel();
            
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("UI Stats"))
        {
            if (Config.DebugEnabled || Version.IsDevelopment)
                ImGui.MenuItem($"Delta: {delta}", false);
            
            var framerate = ImGui.GetIO().Framerate;
            ImGui.MenuItem($"{framerate:###} FPS ({1000f / framerate:0.##} ms/frame)", false);
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
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        // default is a meaningless case here i dont fucking care rider
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

        if (State.Client.ConnectionState == ConnectionState.Connected)
        {
            if (ImGui.BeginMenu($"Bot status:                {State.Client.Status}"))
            {
                if (ImGui.MenuItem("Online", State.Client.Status != UserStatus.Online)) 
                    Await(() => State.Client.SetStatusAsync(UserStatus.Online));
                if (ImGui.MenuItem("Idle", State.Client.Status != UserStatus.Idle)) 
                    Await(() => State.Client.SetStatusAsync(UserStatus.Idle));
                if (ImGui.MenuItem("Do Not Disturb", State.Client.Status != UserStatus.DoNotDisturb)) 
                    Await(() => State.Client.SetStatusAsync(UserStatus.DoNotDisturb));
                if (ImGui.MenuItem("Invisible", State.Client.Status != UserStatus.Invisible)) 
                    Await(() => State.Client.SetStatusAsync(UserStatus.Invisible));
            
                ImGui.EndMenu();
            }
        }
    }

    private void Await(Func<Task> task) => TaskQueue.Enqueue(task);
    private void Await(Task task) => Await(() => task);
}
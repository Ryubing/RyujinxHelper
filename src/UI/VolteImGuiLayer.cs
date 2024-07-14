using System.Collections.Immutable;
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
    
    public ulong SelectedGuildId { get; set; }
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
            ImGui.Begin("Guild Manager");
            GuildManager();
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

            var framerate = Io.Framerate;
            
            ImGui.MenuItem($"{framerate:###} FPS ({1000f / framerate:0.##} ms/frame)", false);
            ImGui.EndMenu();
        }
    }

    public void UiSettings()
    {
        ImGui.Text("Background");
        ImGui.ColorPicker3("", ref State.Background, ImGuiColorEditFlags.NoSidePreview | ImGuiColorEditFlags.NoLabel);
        if (ImGui.SmallButton("Reset"))
            State.Background = ImGuiLayerState.DefaultBackground;
        //ImGui.Separator();
    }

    public void BotManagement()
    {
        ImGui.Text("Discord Gateway:");
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        // default is a meaningless case here i dont fucking care rider
        switch (State.Client.ConnectionState)
        {
            case ConnectionState.Connected:
                ColoredText("  Connected", Color.LawnGreen);
                break;
            case ConnectionState.Connecting:
                ColoredText("  Connecting...", Color.Yellow);
                break;
            case ConnectionState.Disconnecting:
                ColoredText("  Disconnecting...", Color.OrangeRed);
                break;
            case ConnectionState.Disconnected:
                ColoredText("  Disconnected!", Color.Red);
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

    private void GuildSelect()
    {
        if (ImGui.BeginMenu("Select a Guild           ")) // cheap hack to make the initial pane wider
        {
            State.Client.Guilds.ForEach(guild =>
            {
                if (ImGui.MenuItem(guild.Name, guild.Id != State.SelectedGuildId))
                    State.SelectedGuildId = guild.Id;
            });
            ImGui.EndMenu();
        }
    }
    
    public void GuildManager()
    {
        if (State.SelectedGuildId == 0) GuildSelect();
        else
        {
            if (State.SelectedGuildId != 0)
            {
                var selectedGuild = State.Client.GetGuild(State.SelectedGuildId);
                var selectedGuildMembers = selectedGuild.Users.ToImmutableArray();
                var botMembers = selectedGuildMembers.Count(sgu => sgu.IsBot);
                var realMembers = selectedGuildMembers.Length - botMembers;
            
                ImGui.Text(selectedGuild.Name);
                ImGui.Text($"Owner: @{selectedGuild.Owner}");
                ImGui.Text($"Text Channels: {selectedGuild.TextChannels.Count}");
                ImGui.Text($"Voice Channels: {selectedGuild.VoiceChannels.Count}");
                ImGui.Text($"{selectedGuildMembers.Length} members");
                ColoredText($" + {realMembers} users", Color.LawnGreen);
                ColoredText($" - {botMembers} bots", Color.OrangeRed);
            }
            
            ImGui.Separator();
            
            GuildSelect();
        }
    }
    
    private static void ColoredText(string fmt, Color color) =>
        ImGui.TextColored(color.AsVec4(), fmt);
    
}
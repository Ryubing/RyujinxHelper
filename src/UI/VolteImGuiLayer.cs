using System.Collections.Immutable;
using ImGuiNET;
using Silk.NET.Input;
using Color = System.Drawing.Color;

namespace Volte.UI;

public sealed class VolteImGuiState : ImGuiLayerState
{
    public VolteImGuiState(IServiceProvider provider)
    {
        Cts = provider.Get<CancellationTokenSource>();
        Client = provider.Get<DiscordSocketClient>();
        Messages = provider.Get<MessageService>();
        Database = provider.Get<DatabaseService>();
    }

    public CancellationTokenSource Cts { get; }
    public DiscordSocketClient Client { get; }
    public MessageService Messages { get; }
    public DatabaseService Database { get; }
    
    public ulong SelectedGuildId { get; set; }
}

public class VolteImGuiLayer : ImGuiLayer<VolteImGuiState>
{
    public VolteImGuiLayer(IServiceProvider provider)
    {
        State = new VolteImGuiState(provider);
        
        Panels.Add("UI Settings", UiSettings);
        Panels.Add("Command Stats", CommandStats);
        Panels.Add("Bot Management", BotManagement);
        Panels.Add("Guild Manager", GuildManager);
    }

    public readonly Dictionary<string, Action<double>> Panels = new();
    
    public override void Render(double delta)
    {
        if (!VolteBot.IsRunning) return;
        
        {
            if (ImGui.BeginMainMenuBar())
            {
                MenuBar(delta);
                ImGui.EndMainMenuBar();
            }
        }

        foreach (var (panelName, renderPanel) in Panels)
        {
            ImGui.Begin(panelName);
            renderPanel(delta);
            ImGui.End();
        }
    }

    private void CommandStats(double _)
    {
        ImGui.Text($"Total executions: {State.Messages.AllTimeCommandCalls}");
        ColoredText($"  - Successful: {State.Messages.AllTimeSuccessfulCommandCalls}", Color.LawnGreen);
        ColoredText($"  - Failed: {State.Messages.AllTimeFailedCommandCalls}", Color.OrangeRed);
        ImGui.SeparatorText("This Session");
        ImGui.Text($"Executions: {State.Messages.FailedCommandCalls + State.Messages.SuccessfulCommandCalls}");
        ColoredText($"  - Successful: {State.Messages.SuccessfulCommandCalls}", Color.LawnGreen);
        ColoredText($"  - Failed: {State.Messages.FailedCommandCalls}", Color.OrangeRed);
        ImGui.Separator();
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
                
                var process = Process.GetCurrentProcess();
                ImGui.MenuItem($"Process memory: {process.GetMemoryUsage()} ({process.GetMemoryUsage(MemoryType.Kilobytes)})", false);
            }
            
            ImGui.EndMenu();
        }
    }

    private void UiSettings(double _)
    {
        ImGui.Text("Background");
        ImGui.ColorPicker3("", ref State.Background, ImGuiColorEditFlags.NoSidePreview | ImGuiColorEditFlags.NoLabel);
        if (ImGui.SmallButton("Reset"))
            State.Background = ImGuiLayerState.DefaultBackground;
        //ImGui.Separator();
    }

    private void BotManagement(double _)
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
            ImGui.Text($"Connected as: {State.Client.CurrentUser.Username}#{State.Client.CurrentUser.DiscriminatorValue}");
            // ToString()ing the CurrentUser has weird question marks on both sides of Volte-dev's name,
            // so we do it manually in case that happens on other bot accounts too
            
            if (ImGui.BeginMenu($"Bot status: {State.Client.Status}"))
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

    #region Guild Manager Panel

    private void GuildManager(double _)
    {
        if (State.SelectedGuildId != 0)
        {
            var selectedGuild = State.Client.GetGuild(State.SelectedGuildId);
            var selectedGuildMembers = selectedGuild.Users.ToImmutableArray();
            var botMembers = selectedGuildMembers.Count(sgu => sgu.IsBot);
            
            ImGui.Text(selectedGuild.Name);
            ImGui.Text($"Owner: @{selectedGuild.Owner}");
            ImGui.Text($"Text Channels: {selectedGuild.TextChannels.Count}");
            ImGui.Text($"Voice Channels: {selectedGuild.VoiceChannels.Count}");
            ImGui.Text($"{selectedGuildMembers.Length} members");
            ColoredText($" + {selectedGuildMembers.Length - botMembers} users", Color.LawnGreen);
            ColoredText($" - {botMembers} bots", Color.OrangeRed);
            ImGui.Separator();

            var destructiveMenuEnabled = AllKeysPressed(Key.ShiftLeft, Key.ControlLeft);
                
            if (ImGui.BeginMenu("Destructive Actions (Shift + Ctrl)", destructiveMenuEnabled))
            {
                if (ImGui.MenuItem("Leave Guild", destructiveMenuEnabled))
                {
                    Await(() => selectedGuild.LeaveAsync());
                    State.SelectedGuildId = 0; //resets this pane back to just the "select a guild" button
                }

                if (ImGui.MenuItem("Reset Configuration", destructiveMenuEnabled))
                    State.Database.Save(GuildData.CreateFrom(selectedGuild));
                
                ImGui.EndMenu();
            }
                
            ImGui.Separator();
        }

        GuildSelect();
    }
    
    private void GuildSelect()
    {
        if (ImGui.BeginMenu("Select a Guild"))
        {
            State.Client.Guilds.ForEach(guild =>
            {
                if (ImGui.MenuItem(guild.Name, guild.Id != State.SelectedGuildId))
                    State.SelectedGuildId = guild.Id;
            });
            ImGui.EndMenu();
        }
    }

    #endregion
    
    private static void ColoredText(string fmt, Color color) =>
        ImGui.TextColored(color.AsVec4(), fmt);
    
}
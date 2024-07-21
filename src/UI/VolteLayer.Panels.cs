using System.Collections.Immutable;
using ImGuiNET;
using Silk.NET.Input;
using Color = System.Drawing.Color;

namespace Volte.UI;

public partial class VolteUiLayer
{
    private void CommandStats(double _)
    {
        ImGui.Text($"Total executions: {State.Messages.AllTimeCommandCalls}");
        ColoredText($"  - Successful: {State.Messages.AllTimeSuccessfulCommandCalls}", Color.LawnGreen);
        ColoredText($"  - Failed: {State.Messages.AllTimeFailedCommandCalls}", Color.OrangeRed);
        ImGui.SeparatorText("This Session");
        ImGui.Text($"Executions: {
            CalledCommandsInfo.ThisSessionSuccess + CalledCommandsInfo.ThisSessionFailed + 
            State.Messages.UnsavedFailedCommandCalls + State.Messages.UnsavedSuccessfulCommandCalls
        }");
        ColoredText($"  - Successful: {CalledCommandsInfo.ThisSessionSuccess + State.Messages.UnsavedSuccessfulCommandCalls}", Color.LawnGreen);
        ColoredText($"  - Failed: {CalledCommandsInfo.ThisSessionFailed + State.Messages.UnsavedFailedCommandCalls}", Color.OrangeRed);
        ImGui.Separator();
    }

    private void UiSettings(double _)
    {
        ImGui.Text("Background");
        ImGui.ColorPicker3("", ref State.Background, ImGuiColorEditFlags.NoSidePreview | ImGuiColorEditFlags.NoLabel);
        if (ImGui.SmallButton("Reset"))
            State.Background = UiLayerState.DefaultBackground;
        ImGui.Separator();
        if (ImGui.Button("Swap Theme"))
        {
            State.SelectedTheme = !State.SelectedTheme;
            if (State.SelectedTheme) SetColors(ref Spectrum.Dark);
            else SetColors(ref Spectrum.Light);
        }

        if (ImGui.RadioButton("Show Style Editor", State.ShowStyleEditor))
            State.ShowStyleEditor = !State.ShowStyleEditor;
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
            
            var currentStatus = State.Client.Status;
            
            if (ImGui.BeginMenu($"Bot status: {currentStatus}"))
            {
                if (ImGui.MenuItem("Online", currentStatus != UserStatus.Online)) 
                    Await(() => State.Client.SetStatusAsync(UserStatus.Online));
                if (ImGui.MenuItem("Idle", currentStatus != UserStatus.Idle)) 
                    Await(() => State.Client.SetStatusAsync(UserStatus.Idle));
                if (ImGui.MenuItem("Do Not Disturb", currentStatus != UserStatus.DoNotDisturb)) 
                    Await(() => State.Client.SetStatusAsync(UserStatus.DoNotDisturb));
                if (ImGui.MenuItem("Invisible", currentStatus != UserStatus.Invisible)) 
                    Await(() => State.Client.SetStatusAsync(UserStatus.Invisible));
            
                ImGui.EndMenu();
            }
        }
        
        var process = Process.GetCurrentProcess();
        ImGui.Text($"Process memory: {process.GetMemoryUsage()} ({process.GetMemoryUsage(MemoryType.Kilobytes)})");

        if (ImGui.Button("Reload Config"))
            Config.Reload();
    }

    #region Guild Manager

    private void GuildManager(double _)
    {
        if (State.SelectedGuildId != 0)
        {
            var selectedGuild = State.Client.GetGuild(State.SelectedGuildId);
            var selectedGuildMembers = selectedGuild.Users.ToImmutableArray();
            var botMembers = selectedGuildMembers.Count(sgu => sgu.IsBot);
            
            ImGui.SeparatorText(selectedGuild.Name);
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

    #endregion Guild Manager
}
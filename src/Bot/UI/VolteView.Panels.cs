using System.Collections.Immutable;
// needed for commented code //using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Color = System.Drawing.Color;

namespace Volte.UI;

public partial class VolteUiView
{
    private void CommandStats(double _)
    {
        //using var __ = PushStyle(ImGuiStyleVar.WindowMinSize, new Vector2(201, 188));
        
        ImGui.Text($"Total executions: {_state.Messages.AllTimeCommandCalls}");
        ColoredText($"  - Successful: {_state.Messages.AllTimeSuccessfulCommandCalls}", Color.LawnGreen);
        ColoredText($"  - Failed: {_state.Messages.AllTimeFailedCommandCalls}", Color.OrangeRed);
        ImGui.SeparatorText("This Session");
        ImGui.Text($"Executions: {
            CalledCommandsInfo.ThisSessionSuccess + CalledCommandsInfo.ThisSessionFailed + 
            _state.Messages.UnsavedFailedCommandCalls + _state.Messages.UnsavedSuccessfulCommandCalls
        }");
        ColoredText($"  - Successful: {CalledCommandsInfo.ThisSessionSuccess + _state.Messages.UnsavedSuccessfulCommandCalls}", Color.LawnGreen);
        ColoredText($"  - Failed: {CalledCommandsInfo.ThisSessionFailed + _state.Messages.UnsavedFailedCommandCalls}", Color.OrangeRed);
    }

    private void BotManagement(double _)
    {
        //using var __ = PushStyle(ImGuiStyleVar.WindowMinSize, new Vector2(385, 299));
        ImGui.Text("Discord Gateway:");
        ImGui.SameLine();
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        // default is a meaningless case here i dont fucking care rider
        switch (_state.Client.ConnectionState)
        {
            case ConnectionState.Connected:
                ColoredText("Connected", Color.LawnGreen);
                break;
            case ConnectionState.Connecting:
                ColoredText("Connecting...", Color.Yellow);
                break;
            case ConnectionState.Disconnecting:
                ColoredText("Disconnecting...", Color.OrangeRed);
                break;
            case ConnectionState.Disconnected:
                ColoredText("Disconnected!", Color.Red);
                break;
        }

        if (_state.Client.ConnectionState == ConnectionState.Connected)
        {
            ImGui.Text($"Connected as: {_state.Client.CurrentUser.Username}#{_state.Client.CurrentUser.DiscriminatorValue}");
            // ToString()ing the CurrentUser has weird question marks on both sides of Volte-dev's name,
            // so we do it manually in case that happens on other bot accounts too
            
            var currentStatus = _state.Client.Status;
            
            if (ImGui.BeginMenu($"Bot status: {currentStatus}"))
            {
                if (ImGui.MenuItem("Online", currentStatus != UserStatus.Online)) 
                    Await(() => _state.Client.SetStatusAsync(UserStatus.Online));
                if (ImGui.MenuItem("Idle", currentStatus != UserStatus.Idle)) 
                    Await(() => _state.Client.SetStatusAsync(UserStatus.Idle));
                if (ImGui.MenuItem("Do Not Disturb", currentStatus != UserStatus.DoNotDisturb)) 
                    Await(() => _state.Client.SetStatusAsync(UserStatus.DoNotDisturb));
                if (ImGui.MenuItem("Invisible", currentStatus != UserStatus.Invisible)) 
                    Await(() => _state.Client.SetStatusAsync(UserStatus.Invisible));
            
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
        //using var __ = PushStyle(ImGuiStyleVar.WindowMinSize, new Vector2(418, 300));
        
        if (_state.SelectedGuildId != 0)
        {
            var selectedGuild = _state.Client.GetGuild(_state.SelectedGuildId);
            var selectedGuildMembers = selectedGuild.Users.ToImmutableArray();
            var botMembers = selectedGuildMembers.Count(sgu => sgu.IsBot);
            
            ImGui.SeparatorText(selectedGuild.Name);
            ImGui.Text($"Owner: @{selectedGuild.Owner}");
            ImGui.Text($"Text Channels: {selectedGuild.TextChannels.Count}");
            ImGui.Text($"Voice Channels: {selectedGuild.VoiceChannels.Count}");
            
            ImGui.Text($"{selectedGuildMembers.Length} members |"); 
            ImGui.SameLine();
            ColoredText($"{selectedGuildMembers.Length - botMembers} users", Color.LawnGreen); 
            ImGui.SameLine();
            ImGui.Text("|"); 
            ImGui.SameLine();
            ColoredText($"{botMembers} bots", Color.OrangeRed);
            
            ImGui.Separator();

            var destructiveMenuEnabled = AllKeysPressed(Key.ShiftLeft, Key.ControlLeft);
                
            if (ImGui.BeginMenu("Destructive Actions (Shift + Ctrl)", destructiveMenuEnabled))
            {
                if (ImGui.MenuItem("Leave Guild", destructiveMenuEnabled))
                {
                    Await(() => selectedGuild.LeaveAsync());
                    _state.SelectedGuildId = 0; //resets this pane back to just the "select a guild" button
                }

                if (ImGui.MenuItem("Reset Configuration", destructiveMenuEnabled))
                    _state.Database.Save(GuildData.CreateFrom(selectedGuild));
                
                ImGui.EndMenu();
            }
                
            ImGui.Separator();
        }

        GuildSelect();
    }
    
    private void GuildSelect()
    {
        if (!ImGui.BeginMenu("Select a Guild")) return;
        
        _state.Client.Guilds.ForEach(guild =>
        {
            if (ImGui.MenuItem(guild.Name, guild.Id != _state.SelectedGuildId))
                _state.SelectedGuildId = guild.Id;
        });
        
        ImGui.EndMenu();
    }

    #endregion Guild Manager
}
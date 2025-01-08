using Avalonia.Controls.Notifications;
using Gommon;
using MenuFactory.Abstractions.Attributes;
using RyuBot.Helpers;
using RyuBot.Interactions;
using RyuBot.Services;

// ReSharper disable UnusedMember.Global
// These members are never directly invoked.

namespace RyuBot.UI.Avalonia.Pages;

// ReSharper disable once InconsistentNaming
public class ShellViewMenu
{
    [Menu("Clear Commands", "Dev", Icon = "fa-solid fa-broom")]
    public static async Task ClearCommands()
    {
        var interactionService = RyujinxBot.Services.Get<RyujinxBotInteractionService>();
        if (interactionService is null || RyujinxBot.Client is null)
        {
            RyujinxBotApp.Notify("Not logged in", "State error", NotificationType.Error);
            return;
        }

#if DEBUG
        var removedCommandsText = $"{await interactionService.ClearAllCommandsInGuildAsync(DiscordHelper.DevGuildId)} commands removed";
#else
        var removedCount = 0;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var guildId in Config.WhitelistGuilds)
        {
            removedCount = Math.Max(await interactionService.ClearAllCommandsInGuildAsync(guildId), removedCount);
        }
        var removedCommandsText = $"{removedCount} commands removed from {Config.WhitelistGuilds.Count()} guilds.";
#endif

        RyujinxBotApp.Notify(removedCommandsText, "Interaction commands cleared");
    }

    [Menu("Clean Compat List", "Dev", Icon = "fa-solid fa-broom")]
    public static Task CleanCompatList()
    {
        var compatCsv = RyujinxBot.Services.Get<CompatibilityCsvService>()?.Csv;
        if (compatCsv is null || RyujinxBot.Client is null)
        {
            RyujinxBotApp.Notify("Not logged in", "State error", NotificationType.Error);
            return Task.CompletedTask;
        }

        var dt = DateTime.Now;
        var fp = FilePath.Data / "compat" / $"clean-{dt.Year}-{dt.Month}-{dt.Day}-{dt.Ticks}.csv";
        
        compatCsv.Export(fp);
        
        RyujinxBotApp.Notify($"Exported cleaned CSV to {fp.Path}");

        return Task.CompletedTask;
    }
}
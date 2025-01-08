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
        var removedCommands = await interactionService.ClearAllCommandsInGuildAsync(DiscordHelper.DevGuildId);
#else
        var removedCommands = 0;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var guildId in Config.WhitelistGuilds)
        {
            removedCommands = Math.Max(await interactionService.ClearAllCommandsInGuildAsync(guildId), removedCommands);
        }
#endif

        RyujinxBotApp.Notify($"{removedCommands} removed", "Interaction commands cleared");
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
        var fp = FilePath.Data / "compat" / $"{dt.Year}-{dt.Month}-{dt.Day}.csv";
        
        compatCsv.Export(fp);
        
        RyujinxBotApp.Notify($"Exported cleaned CSV to {fp.Path}");

        return Task.CompletedTask;
    }
}
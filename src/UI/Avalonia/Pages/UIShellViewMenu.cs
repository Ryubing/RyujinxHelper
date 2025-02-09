using System.Text.Json;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
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

    [Menu("Export TitleDB", "Dev", Icon = "fa-solid fa-file-export")]
    public static async Task ExportTitleDb()
    {
        var pickedFile = await RyujinxBotApp.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Nintendo Switch Online RomFS TitleDB")
                {
                    Patterns = ["*.titlesdb"]
                }
            ]
        });

        if (pickedFile.None())
            return;

        var jdoc = JsonDocument.Parse(await File.ReadAllTextAsync(pickedFile[0].Path.AbsolutePath));

        Dictionary<string, List<string>> gameNamesToTitleIds = [];

        foreach (var jsonProperty in jdoc.RootElement.GetProperty("titles").EnumerateObject())
        {
            var title = jsonProperty.Value.GetProperty("title").GetString()!;
            
            if (!gameNamesToTitleIds.ContainsKey(title))
                gameNamesToTitleIds.Add(title, []);
            
            gameNamesToTitleIds[title].Add(jsonProperty.Name.Replace("-", "_"));
        }

        var fp = FilePath.Data / "titledb" / $"clean-{Path.GetFileName(pickedFile[0].Path.AbsolutePath)}";
            
        if (fp.TryGetParent(out var parent) && !parent.ExistsAsDirectory)
            Directory.CreateDirectory(parent.Path);
        
        fp.WriteAllLines(gameNamesToTitleIds
            .Select(kvp => 
                $"{kvp.Value.Select(x => $"\"{x.ToLower()}\"").JoinToString(" or ")} => Playing(\"{kvp.Key}\"),"
            ));
        
        RyujinxBotApp.Notify($"Exported TitleDB to {fp.Path}");
    }
}
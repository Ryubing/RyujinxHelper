﻿using Discord.Interactions;

namespace RyuBot.Interactions.Commands.Modules;

public partial class CompatibilityModule
{
    [SlashCommand("compatibility", "Show compatibility information for a game.")]
    public async Task<RuntimeResult> CompatibilityAsync(
        [Summary("game_title", "The name of the game to lookup.")]
        [Autocomplete(typeof(GameCompatibilityNameAutocompleter))]
        string gameName)
    {
        if (Compatibility.GetByGameName(gameName) is not { } csvEntry)
            return BadRequest($"Could not find a game compatibility entry for `{gameName}`.");
        
        return Ok(Context.CreateReplyBuilder(true)
            .WithEmbed(embed =>
            {
                embed.WithTitle(csvEntry.GameName.Truncate(EmbedBuilder.MaxTitleLength));
                embed.AddField("Status", Capitalize(csvEntry.PlayabilityStatus));
                embed.AddField("Title ID", csvEntry.TitleId);
                embed.AddField("Last Updated", csvEntry.LastEvent.FormatPrettyString());
                embed.WithFooter(csvEntry.IssueLabels);
                embed.WithColor(csvEntry.PlayabilityStatus.ToLower() switch
                {
                    "nothing" or "boots" or "menus" => Color.Red,
                    "ingame" => System.Drawing.Color.Yellow.Into(c => new Color(c.R, c.G, c.B)),
                    _ => Color.Green
                });
                embed.WithCurrentTimestamp();
            }));
    }

    private static string Capitalize(string value)
    {
        if (value == string.Empty)
            return string.Empty;
        
        var firstChar = value[0];
        var rest = value[1..];

        return $"{char.ToUpper(firstChar)}{rest}";
    }
}

public class GameCompatibilityNameAutocompleter : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context, 
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter, 
        IServiceProvider services)
    {
        var compatCsv = services.Get<CompatibilityCsvService>().Csv;
        
        foreach (var option in autocompleteInteraction.Data.Options)
        {
            if (option.Focused && !string.Empty.Equals(option.Value))
            {
                var userValue = option.Value.ToString();
                var results = compatCsv.Entries.Where(x => x.GameName.ContainsIgnoreCase(userValue)).Take(25).ToArray();
                if (results.Length > 0)
                {
                    return Task.FromResult(AutocompletionResult.FromSuccess(
                        results.Select(it => new AutocompleteResult(it.GameName, it.GameName))));
                }
            }
        }
        return Task.FromResult(AutocompletionResult.FromSuccess());
    }
}
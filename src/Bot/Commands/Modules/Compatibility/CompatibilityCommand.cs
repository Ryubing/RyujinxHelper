using System.Globalization;
using Discord.Interactions;

namespace RyuBot.Commands.Modules;

public partial class CompatibilityModule
{
    [SlashCommand("compatibility", "Show compatibility information for a game.")]
    public Task<RuntimeResult> CompatibilityAsync(
        [Summary("game", "The name or title ID of the game to lookup.")]
        [Autocomplete(typeof(GameCompatibilityNameAutocompleter))] 
        string game,
        [Summary("public", "Post the compatibility result publicly.")]
        bool publicResult = false
        )
    {
        var searchedForTitleId = ulong.TryParse(game, NumberStyles.HexNumber, null, out _);
        
        if (!Compatibility.FindOrNull(game).TryGet(out var csvEntry))
            return BadRequest($"Could not find a game compatibility entry for `{game}`. {
                (searchedForTitleId ? "Try specifying a name instead of ID!" : string.Empty)
            }".TrimEnd());
        
        return Ok(CreateReplyBuilder(!publicResult)
            .WithEmbed(embed =>
            {
                embed.WithTitle(csvEntry.GameName.Truncate(EmbedBuilder.MaxTitleLength));
                embed.AddField("Status", csvEntry.Status.Capitalize(), true);
                csvEntry.TitleId.IfPresent(tid
                    => embed.AddField("Title ID", tid, true)
                );
                embed.AddField("Labels", csvEntry.IssueLabels
                    .Where(it => !it.StartsWithIgnoreCase("status"))
                    .Select(GitHubHelper.FormatLabelName).JoinToString(", ")
                );
                embed.WithColor(csvEntry.Status.ToLower() switch
                {
                    "nothing" or "boots" or "menus" => Color.Red,
                    "ingame" => System.Drawing.Color.Yellow.Into(c => new Color(c.R, c.G, c.B)),
                    _ => Color.Green
                });
                embed.WithTimestamp(csvEntry.LastEvent);
            }));
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
        var compatCsv = services.Get<CompatibilityCsvService>();
        
        foreach (var option in autocompleteInteraction.Data.Options)
        {
            if (option.Focused && !string.Empty.Equals(option.Value))
            {
                var userValue = option.Value.ToString();
                var results = compatCsv.SearchEntries(userValue).Take(25).ToArray();
                
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
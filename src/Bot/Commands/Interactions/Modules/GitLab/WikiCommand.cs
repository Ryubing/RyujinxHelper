using Discord.Interactions;
using NGitLab.Models;

namespace RyuBot.Commands.Interactions.Modules;

public partial class GitLabModule
{
    [SlashCommand("wiki", "Find a wiki page on the Ryubing project page on the Ryubing GitLab.")]
    public Task<RuntimeResult> WikiAsync(
        [Summary("page", "The slug or title of the wiki page to lookup.")] [Autocomplete(typeof(WikiPageAutocompleter))]
        string page,
        [Summary("public", "Post the wiki page result publicly.")]
        bool publicResult = false)
    {
        if (!GitLab.SearchWikiPages(page).FindFirst().TryGet(out var wikiPage))
            return BadRequest(
                new StringBuilder()
                    .AppendLine($"Could not find a wiki page entry for `{page}`.")
                    .AppendLine(
                        "Please wait for the autocomplete suggestions to fill in if you aren't sure what to put!")
                    .ToString()
            );

        return Ok(CreateReplyBuilder(!publicResult).WithContent(Format.Url(wikiPage.Title, GetUrl(wikiPage))));
    }

    private string GetUrl(WikiPage wikiPage) => $"https://git.ryujinx.app/ryubing/ryujinx/-/wikis/{wikiPage.Slug}";
}

public class WikiPageAutocompleter : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        foreach (var option in autocompleteInteraction.Data.Options)
        {
            var userValue = option.Value?.ToString();

            if (!option.Focused || string.Empty.Equals(userValue)) continue;

            var results = services.Get<GitLabService>()
                .SearchWikiPages(userValue).Take(5).ToArray();

            if (results.Length > 0)
                return Task.FromResult(AutocompletionResult.FromSuccess(
                    results.Select(it => new AutocompleteResult(it.Title, it.Slug))
                ));
        }

        return Task.FromResult(AutocompletionResult.FromSuccess(
            services.Get<GitLabService>().GetWikiPages().Select(it => new AutocompleteResult(it.Title, it.Slug))
        ));
    }
}
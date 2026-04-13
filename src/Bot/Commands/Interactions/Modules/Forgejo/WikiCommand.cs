using Discord.Interactions;

namespace RyuBot.Commands.Interactions.Modules;

public partial class ForgejoModule
{
    [SlashCommand("wiki", "Find a wiki page on the Ryubing project page on the Ryubing Forgejo.")]
    public Task<RuntimeResult> WikiAsync(
        [Summary("page", "The slug or title of the wiki page to lookup.")] [Autocomplete(typeof(WikiPageAutocompleter))]
        string page,
        [Summary("public", "Post the wiki page result publicly.")]
        bool publicResult = false)
    {
        if (!Forgejo.SearchWikiPages(page).FindFirst().TryGet(out var wikiPage))
            return BadRequest(
                new StringBuilder()
                    .AppendLine($"Could not find a wiki page entry for `{page}`.")
                    .AppendLine(
                        "Please wait for the autocomplete suggestions to fill in if you aren't sure what to put!")
                    .ToString()
            );

        return Ok(CreateReplyBuilder(!publicResult)
            .WithContent($"View the `{wikiPage.title}` wiki page {Format.Url("here", wikiPage.html_url)}.")
        );
    }
}
using Discord.Interactions;

namespace RyuBot.Interactions.Commands.Modules;

public partial class GitHubModule
{
    [SlashCommand("stars", "Show the amount of stars on the Ryujinx GitHub repository.")]
    public async Task<RuntimeResult> StarCountAsync(
        [Summary("public", "Post the response publicly.")]
        bool publicResponse = false)
    {
        var repo = await GitHub.GetRepositoryAsync(Context);

        return Ok(
            $"{Format.Url($"{repo.Owner.Login}/{repo.Name}", repo.HtmlUrl)} has {repo.StargazersCount} {Emojis.Star}!",
            ephemeral: !publicResponse
        );
    }
}
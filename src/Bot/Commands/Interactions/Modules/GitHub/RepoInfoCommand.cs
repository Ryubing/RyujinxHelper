using Discord.Interactions;
using RyuBot.Interactions;

namespace RyuBot.Commands.Interactions.Modules;

public partial class GitHubModule
{
    [SlashCommand("repo-info", "Shows information about the Ryujinx GitHub repository.")]
    public async Task<RuntimeResult> RepoInfoAsync()
    {
        var repo = await GitHub.GetRepositoryAsync(Context);
        
        return Ok(CreateReplyBuilder(true)
            .WithButtons(Buttons.Link(repo.HtmlUrl, "Open on GitHub"))
            .WithEmbed(embed =>
            {
                embed.WithAuthor(repo.Owner.Login, repo.Owner.AvatarUrl, repo.Owner.HtmlUrl);
                embed.WithTitle(repo.FullName);
                embed.WithDescription(repo.Description);
                if (repo.Topics.Count > 0)
                    embed.AddField("Topics", repo.Topics.JoinToString(", "));
                embed.AddField("Community",
                    $"{repo.StargazersCount} {Emojis.Star} | {repo.SubscribersCount} {Emojis.Eyes} | {repo.ForksCount} forks");
                embed.AddField("Repo Size", repo.Size.Kilobytes(), true);
                embed.AddField("Created", repo.CreatedAt.ToDiscordTimestamp(TimestampType.Relative), true);
                embed.WithFooter(repo.License.Name);
            }));
    }
}
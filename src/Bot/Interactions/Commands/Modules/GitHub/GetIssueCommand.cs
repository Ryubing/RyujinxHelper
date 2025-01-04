using Discord.Interactions;
using Octokit;
using RyuBot.Interactions;

namespace Volte.Interactions.Commands.Modules;

public partial class GitHubModule
{
    [SlashCommand("issue", "Show the title and description of an issue on the Ryujinx GitHub.")]
    public async Task<RuntimeResult> IssueAsync([Summary("issue_number", "The issue to display.")] int issueNumber)
    {
        var issue = await GitHub.GetIssueAsync(Context, issueNumber);

        if (issue is null)
            return BadRequest($"Issue {issueNumber} not found.");

        return Ok(Context.CreateReplyBuilder()
            .WithEmbed(embed =>
            {
                embed.WithAuthor(issue.User.Name, issue.User.AvatarUrl, issue.User.Url);
                embed.WithTitle($"[{issue.Number}] {issue.Title}".Truncate(EmbedBuilder.MaxTitleLength));
                embed.WithDescription(issue.Body.Truncate(EmbedBuilder.MaxDescriptionLength));
                embed.WithColor(GetColorBasedOnIssueState(issue));
                embed.AddField("Labels", issue.Labels.Select(x => x.Name).JoinToString(", "));
                if (issue.UpdatedAt is { } dto)
                    embed.WithTimestamp(dto);
            }));
    }

    private Color GetColorBasedOnIssueState(Issue issue) =>
        IsIssueOpen(issue)
            ? Color.Green 
            : Color.Red;

    private bool IsIssueOpen(Issue issue) => issue.ClosedAt is null;
}
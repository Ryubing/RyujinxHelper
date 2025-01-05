using Discord.Interactions;
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
        
        return Ok(CreateReplyBuilder()
            .WithButtons(Buttons.Link(issue.HtmlUrl, "Open on GitHub"))
            .WithEmbed(embed =>
            {
                embed.WithAuthor(issue.User.Login, issue.User.AvatarUrl, issue.HtmlUrl);
                embed.WithTitle($"[{issue.Number}] {issue.Title}".Truncate(EmbedBuilder.MaxTitleLength));
                embed.AddField("Labels", issue.FormatLabels());
                embed.WithDescription(issue.Body.Truncate(EmbedBuilder.MaxDescriptionLength));
                embed.WithColor(GetColorBasedOnIssueState(issue));
                embed.WithFooter(FormatIssueState(issue));
            }));
    }
}
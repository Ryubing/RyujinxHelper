using Octokit;
using RyuBot.Commands;
using RyuBot.Interactions;

namespace Volte.Interactions.Commands.Modules;

public partial class GitHubModule : RyujinxBotSlashCommandModule
{
    public GitHubService GitHub { get; set; }
    
    private Color GetColorBasedOnIssueState(Issue issue) =>
        IsIssueOpen(issue)
            ? Color.Green 
            : Color.DarkRed;
    
    private Color GetColorBasedOnIssueState(PullRequest issue) =>
        IsIssueOpen(issue)
            ? Color.Green 
            : Color.DarkRed;

    private bool IsIssueOpen(Issue issue) => issue.ClosedAt is null;
    private bool IsIssueOpen(PullRequest issue) => issue.ClosedAt is null;
}
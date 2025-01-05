using Octokit;
using RyuBot.Interactions;

namespace Volte.Interactions.Commands.Modules;

public partial class GitHubModule : RyujinxBotSlashCommandModule
{
    public GitHubService GitHub { get; set; }
    
    private static Color GetColorBasedOnIssueState(Issue issue) =>
        IsIssueOpen(issue)
            ? Color.Green 
            : Color.DarkRed;

    private static Color GetColorBasedOnIssueState(PullRequest pr) =>
        IsIssueOpen(pr)
            ? Color.Green
            : pr.Merged
                ? Color.Purple
                : Color.DarkRed;

    private static bool IsIssueOpen(Issue issue) => issue.ClosedAt is null;
    private static bool IsIssueOpen(PullRequest pr) => pr.ClosedAt is null;
}
using Octokit;
using RyuBot.Interactions;

namespace Volte.Interactions.Commands.Modules;

public partial class GitHubModule : RyujinxBotSlashCommandModule
{
    public static readonly Color OpenColor = new(0x238636);
    public static readonly Color MergedColor = new(0x8957E5);
    public static readonly Color ClosedColor = new(0xDA3633);
    
    public GitHubService GitHub { get; set; }
    
    private static Color GetColorBasedOnIssueState(Issue issue) =>
        IsIssueOpen(issue)
            ? OpenColor
            : ClosedColor;
    
    private static Color GetColorBasedOnIssueState(PullRequest pr) =>
        IsIssueOpen(pr)
            ? OpenColor
            : pr.Merged
                ? MergedColor
                : ClosedColor;
    
    private static string FormatIssueState(Issue issue) =>
        IsIssueOpen(issue)
            ? $"Opened {issue.CreatedAt.FormatPrettyString()}"
            : $"Closed {issue.ClosedAt!.Value.FormatPrettyString()}";
    
    private static string GetIssueState(Issue issue) =>
        IsIssueOpen(issue)
            ? "Open"
            : "Closed";

    private static string FormatIssueState(PullRequest pr) =>
        IsIssueOpen(pr)
            ? $"Opened {pr.CreatedAt.FormatPrettyString()}"
            : pr.Merged
                ? $"Merged {pr.ClosedAt!.Value.FormatPrettyString()}"
                : $"Closed {pr.ClosedAt!.Value.FormatPrettyString()}";
    
    private static string GetIssueState(PullRequest pr) =>
        IsIssueOpen(pr)
            ? "Open"
            : pr.Merged
                ? "Merged"
                : "Closed";

    private static bool IsIssueOpen(Issue issue) => issue.ClosedAt is null;
    private static bool IsIssueOpen(PullRequest pr) => pr.ClosedAt is null;
}
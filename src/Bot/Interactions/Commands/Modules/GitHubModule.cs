using Octokit;
using RyuBot.Interactions.Commands;

namespace Volte.Interactions.Commands.Modules;

public partial class GitHubModule : RyujinxBotSlashCommandModule
{
    public GitHubClient ApiClient { get; set; }
    public GitHubService GitHub { get; set; }
}
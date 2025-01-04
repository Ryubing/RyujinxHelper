using Octokit;
using RyuBot.Interactions.Commands;

namespace Volte.Interactions.Commands.Modules;

public partial class GitHubModule : RyujinxBotSlashCommandModule
{
    public GitHubService GitHub { get; set; }
}
using Discord.Interactions;
using Octokit;

namespace RyuBot.Services;

public class GitHubService : BotService
{
    private const string RepoOwner = "Ryubing";
    private const string RepoName = "Ryujinx";
    
    private readonly GitHubClient _gitHub;

    public GitHubService(GitHubClient ghc)
    {
        _gitHub = ghc;
    }

    private static (string Owner, string Name) GetRepo<T>(SocketInteractionContext<T> ctx) where T : SocketInteraction => 
        ctx.Guild?.Id switch
        {
            1291765437100720243 => ("ryujinx-mirror", "ryujinx"),
            _ => (RepoOwner, RepoName)
        };

    public async Task<Issue> GetIssueAsync<TInteraction>(SocketInteractionContext<TInteraction> ctx, int issueNumber)
        where TInteraction : SocketInteraction
    {
        try
        {
            var (owner, repoName) = GetRepo(ctx);
            return await _gitHub.Issue.Get(owner, repoName, issueNumber);
        }
        catch
        {
            return null;
        }
    }

    public async Task<PullRequest> GetPullRequestAsync<TInteraction>(SocketInteractionContext<TInteraction> ctx, int prNumber) where TInteraction : SocketInteraction
    { 
        try
        {
            var (owner, repoName) = GetRepo(ctx);
            return await _gitHub.PullRequest.Get(owner, repoName, prNumber);
        }
        catch
        {
            return null;
        }
    }

    public Task<Release> GetLatestStableAsync<TInteraction>(SocketInteractionContext<TInteraction> ctx)
        where TInteraction : SocketInteraction
    {
        var (owner, repoName) = GetRepo(ctx);
        return _gitHub.Repository.Release.GetLatest(owner, repoName);
    }

    public Task<Release> GetLatestCanaryAsync()
        => _gitHub.Repository.Release.GetLatest(RepoOwner, "Canary-Releases");

    public Task<IReadOnlyList<User>> GetStargazersAsync<TInteraction>(SocketInteractionContext<TInteraction> ctx)
        where TInteraction : SocketInteraction
    {
        var (owner, repoName) = GetRepo(ctx);
        return _gitHub.Activity.Starring.GetAllStargazers(owner, repoName);
    }
}
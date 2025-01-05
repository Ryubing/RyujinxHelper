using Discord.Interactions;
using GitHubJwt;
using Octokit;

namespace RyuBot.Services;

public class GitHubService : BotService
{
    public GitHubClient ApiClient { get; private set; }
    private readonly PeriodicTimer _githubRefreshTimer = new(59.Minutes());
    private readonly CancellationTokenSource _cts;
    private readonly GitHubJwtFactory _jwtFactory;

    private const long InstallationId = 59135375;
    
    public GitHubService(GitHubJwtFactory jwtFactory, CancellationTokenSource cts)
    {
        _cts = cts;
        _jwtFactory = jwtFactory;
        
        if (Config.GitHubAppInstallationId is not 0)
        {
            Initialize(Config.GitHubAppInstallationId);
        }
        else
        {
            Warn(LogSource.Service, "Skipping initialization for GitHub service due to missing Installation ID in config.");
        }
    }

    public void Initialize(long installationId) => ExecuteBackgroundAsync(async () =>
    {
        await LoginToGithubAsync(installationId);
        
        while (await _githubRefreshTimer.WaitForNextTickAsync(_cts.Token))
        {
            Info(LogSource.Service, "Refreshing GitHub authentication.");
            await LoginToGithubAsync(installationId);
        }
    });

    private GitHubClient CreateTopLevelClient() =>
        new(new ProductHeaderValue("RyujinxHelper", Version.DotNetVersion.ToString()))
        {
            Credentials = new(_jwtFactory.CreateEncodedJwtToken(), AuthenticationType.Bearer)
        };

    private async Task LoginToGithubAsync(long installationId)
    {
        Info(LogSource.Service, "Authenticated with JWT");
        
        var installationToken = await CreateTopLevelClient().GitHubApps.CreateInstallationToken(installationId);
        Info(LogSource.Service, $"Created installation token for ID {installationId}");
        
        ApiClient = new GitHubClient(new ProductHeaderValue($"RyujinxHelper-Installation{installationId}",
            Version.DotNetVersion.ToString()))
        {
            Credentials = new Credentials(installationToken.Token)
        };
    }

    public Task<Issue> GetIssueAsync(IInteractionContext ctx, int issueNumber)
    {
        try
        {
            var (owner, repoName) = GitHubHelper.GetRepo(ctx);
            return ApiClient.Issue.Get(owner, repoName, issueNumber);
        }
        catch
        {
            return Task.FromResult<Issue>(null);
        }
    }

    public Task<PullRequest> GetPullRequestAsync(IInteractionContext ctx, int prNumber)
    { 
        try
        {
            var (owner, repoName) = GitHubHelper.GetRepo(ctx);
            return ApiClient.PullRequest.Get(owner, repoName, prNumber);
        }
        catch
        {
            return Task.FromResult<PullRequest>(null);
        }
    }

    public Task<IReadOnlyList<IssueComment>> GetCommentsForIssueAsync(IInteractionContext ctx, int issueNumber)
    {
        try
        {
            var (owner, repoName) = GitHubHelper.GetRepo(ctx);
            return ApiClient.Issue.Comment.GetAllForIssue(owner, repoName, issueNumber);
        }
        catch
        {
            return Task.FromResult<IReadOnlyList<IssueComment>>([]);
        }
    }

    public Task<Release> GetLatestStableAsync(IInteractionContext ctx)
    {
        try
        {
            var (owner, repoName) = GitHubHelper.GetRepo(ctx);
            return ApiClient.Repository.Release.GetLatest(owner, repoName);
        }
        catch
        {
            return Task.FromResult<Release>(null);
        }
    }

    public Task<Release> GetLatestCanaryAsync()
    {
        try
        {
            return ApiClient.Repository.Release.GetLatest(GitHubHelper.MainRepoOwner, "Canary-Releases");
        }
        catch
        {
            return Task.FromResult<Release>(null);
        }
    }
    
    public Task<Repository> GetRepositoryAsync(IInteractionContext ctx)
    {
        try
        {
            var (owner, repoName) = GitHubHelper.GetRepo(ctx);
            return ApiClient.Repository.Get(owner, repoName);
        }
        catch
        {
            return Task.FromResult<Repository>(null);
        }
    }
}
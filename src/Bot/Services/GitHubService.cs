using Discord.Interactions;
using GitHubJwt;
using Octokit;

namespace RyuBot.Services;

public class GitHubService : BotService
{
    public GitHubClient ApiClient { get; private set; }
    private readonly PeriodicTimer _githubRefreshTimer = new(9.Minutes());
    private readonly CancellationTokenSource _cts;
    private readonly GitHubJwtFactory _jwtFactory;

    private const long InstallationId = 59135375;
    
    public GitHubService(GitHubJwtFactory jwtFactory, CancellationTokenSource cts)
    {
        _cts = cts;
        _jwtFactory = jwtFactory;
        Initialize();
    }

    public void Initialize() => ExecuteBackgroundAsync(async () =>
    {
        await LoginToGithubAsync();
        
        while (await _githubRefreshTimer.WaitForNextTickAsync(_cts.Token))
        {
            Info(LogSource.Service, "Refreshing GitHub authentication.");
            await LoginToGithubAsync();
        }
    });

    private async Task LoginToGithubAsync()
    {
        var topLevelClient = new GitHubClient(new ProductHeaderValue("RyujinxHelper", Version.DotNetVersion.ToString()))
        {
            Credentials = new(_jwtFactory.CreateEncodedJwtToken(), AuthenticationType.Bearer)
        };
        
        Info(LogSource.Service, "Authenticated with JWT");
        
        var installationToken = await topLevelClient.GitHubApps.CreateInstallationToken(InstallationId);
        Info(LogSource.Service, $"Created installation token for ID {InstallationId}");
        
        ApiClient = new GitHubClient(new ProductHeaderValue($"RyujinxHelper-Installation{InstallationId}",
            Version.DotNetVersion.ToString()))
        {
            Credentials = new Credentials(installationToken.Token)
        };
    }

    public async Task<Issue> GetIssueAsync<TInteraction>(SocketInteractionContext<TInteraction> ctx, int issueNumber)
        where TInteraction : SocketInteraction
    {
        try
        {
            var (owner, repoName) = GitHubHelper.GetRepo(ctx);
            return await ApiClient.Issue.Get(owner, repoName, issueNumber);
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
            var (owner, repoName) = GitHubHelper.GetRepo(ctx);
            return await ApiClient.PullRequest.Get(owner, repoName, prNumber);
        }
        catch
        {
            return null;
        }
    }

    public Task<Release> GetLatestStableAsync<TInteraction>(SocketInteractionContext<TInteraction> ctx)
        where TInteraction : SocketInteraction
    {
        var (owner, repoName) = GitHubHelper.GetRepo(ctx);
        return ApiClient.Repository.Release.GetLatest(owner, repoName);
    }

    public Task<Release> GetLatestCanaryAsync()
        => ApiClient.Repository.Release.GetLatest(GitHubHelper.MainRepoOwner, "Canary-Releases");
}
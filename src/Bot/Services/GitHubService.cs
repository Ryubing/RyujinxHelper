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

    public Task<Release> GetLatestStableAsync()
    {
        try
        {
            return ApiClient.Repository.Release.GetLatest(GitHubHelper.MainRepoOwner, "Stable-Releases");
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
}
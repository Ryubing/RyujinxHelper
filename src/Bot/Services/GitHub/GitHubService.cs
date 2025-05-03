using GitHubJwt;
using Octokit;

namespace RyuBot.Services;

public class GitHubService : BotService
{
    private GitHubClient ApiClient { get; set; }
    private readonly HttpClient _http;
    private readonly PeriodicTimer _githubRefreshTimer = new(59.Minutes());
    private readonly CancellationTokenSource _cts;
    private readonly GitHubJwtFactory _jwtFactory;

    private ReleaseChannels _releaseChannels;
    
    public GitHubService(GitHubJwtFactory jwtFactory, CancellationTokenSource cts, HttpClient httpClient)
    {
        _cts = cts;
        _jwtFactory = jwtFactory;
        _http = httpClient;
        
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
        _releaseChannels = await GetReleaseChannelsAsync();
        
        while (await _githubRefreshTimer.WaitForNextTickAsync(_cts.Token))
        {
            Info(LogSource.Service, "Refreshing GitHub authentication.");
            await LoginToGithubAsync(installationId);
            _releaseChannels = await GetReleaseChannelsAsync();
        }
    });

    private async Task<ReleaseChannels> GetReleaseChannelsAsync()
        => new(JsonSerializer.Deserialize(await _http.GetStringAsync("https://ryujinx.app/api/release-channels"), ReleaseChannelPairContext.Default.ReleaseChannelPair)); 

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
    
    public Task<Release> GetStableReleaseAsync(string tag)
    {
        try
        {
            return _releaseChannels.Stable.GetReleaseAsync(ApiClient, tag);
        }
        catch
        {
            return Task.FromResult<Release>(null);
        }
    }
    
    public Task<Release> GetCanaryReleaseAsync(string tag)
    {
        try
        {
            return _releaseChannels.Canary.GetReleaseAsync(ApiClient, tag);
        }
        catch
        {
            return Task.FromResult<Release>(null);
        }
    }

    public Task<Release> GetLatestStableAsync()
    {
        try
        {
            return _releaseChannels.Stable.GetLatestReleaseAsync(ApiClient);
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
            return _releaseChannels.Canary.GetLatestReleaseAsync(ApiClient);
        }
        catch
        {
            return Task.FromResult<Release>(null);
        }
    }
}
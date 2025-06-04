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

    private GitHubReleaseChannels _gitHubReleaseChannels;
    
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
        _gitHubReleaseChannels = await GetReleaseChannelsAsync();
        
        while (await _githubRefreshTimer.WaitForNextTickAsync(_cts.Token))
        {
            Info(LogSource.Service, "Refreshing GitHub authentication.");
            await LoginToGithubAsync(installationId);
            _gitHubReleaseChannels = await GetReleaseChannelsAsync();
        }
    });

    private async Task<GitHubReleaseChannels> GetReleaseChannelsAsync()
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
            return _gitHubReleaseChannels.Stable.GetReleaseAsync(ApiClient, tag);
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
            return _gitHubReleaseChannels.Canary.GetReleaseAsync(ApiClient, tag);
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
            return _gitHubReleaseChannels.Stable.GetLatestReleaseAsync(ApiClient);
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
            return _gitHubReleaseChannels.Canary.GetLatestReleaseAsync(ApiClient);
        }
        catch
        {
            return Task.FromResult<Release>(null);
        }
    }
    
    public Task<Release> GetLatestKenjinxReleaseAsync()
    {
        try
        {
            var parts = KenjinxReleases.Split('/');
            return ApiClient.Repository.Release.GetLatest(parts[0], parts[1]);
        }
        catch
        {
            return Task.FromResult<Release>(null);
        }
    }
    
    public Task<Release> GetLatestKenjinxAndroidReleaseAsync()
    {
        try
        {
            var parts = KenjinxAndroidReleases.Split('/');
            return ApiClient.Repository.Release.GetLatest(parts[0], parts[1]);
        }
        catch
        {
            return Task.FromResult<Release>(null);
        }
    }
    
    public Task<Release> GetKenjinxReleaseAsync(string tag)
    {
        try
        {
            var parts = KenjinxReleases.Split('/');
            return ApiClient.Repository.Release.Get(parts[0], parts[1], tag);
        }
        catch
        {
            return Task.FromResult<Release>(null);
        }
    }
    
    public Task<Release> GetKenjinxAndroidReleaseAsync(string tag)
    {
        try
        {
            var parts = KenjinxAndroidReleases.Split('/');
            return ApiClient.Repository.Release.Get(parts[0], parts[1], tag);
        }
        catch
        {
            return Task.FromResult<Release>(null);
        }
    }

    public const string KenjinxReleases = "Kenji-NX/Releases";
    public const string KenjinxAndroidReleases = "Kenji-NX/Android-Releases";
}
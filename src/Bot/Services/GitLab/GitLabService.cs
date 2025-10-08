using NGitLab;
using NGitLab.Models;
using Ryujinx.Systems.Update.Client;
using Ryujinx.Systems.Update.Common;

namespace RyuBot.Services;

public class GitLabService : BotService
{
    private readonly PeriodicTimer _gitlabRefreshTimer = new(12.Hours());
    private readonly CancellationTokenSource _cts;
    private readonly HttpClient _http;
    private readonly UpdateClient _updateClient;
    
    private IWikiClient _ryubingWikiClient;

    private Dictionary<string, VersionCacheSource> _releaseChannels;

    private WikiPage[] _cachedPages;
    public WikiPage[] WikiPages => _cachedPages ?? [];
    
    public GitLabClient Client { get; } = new(Config.GitLabAuth.InstanceUrl, Config.GitLabAuth.AccessToken);

    public async Task InitAsync()
    {
        _ryubingWikiClient = Client.GetWikiClient(new ProjectId("ryubing/ryujinx"));

        _cachedPages = _ryubingWikiClient.All.Where(x => x.Slug != "Dumping" && x.Title != "home").ToArray();
        
        Info(LogSource.Service, $"Cached {_cachedPages.Length} wiki pages.");
        
        _releaseChannels = await _updateClient.QueryCacheSourcesAsync();
        while (await _gitlabRefreshTimer.WaitForNextTickAsync(_cts.Token))
        {
            Info(LogSource.Service, "Refreshing GitLab release channels.");
            _releaseChannels = await _updateClient.QueryCacheSourcesAsync();
        }
    } 

    public GitLabService(HttpClient httpClient, CancellationTokenSource cancellationTokenSource)
    {
        _http = httpClient;
        _cts = cancellationTokenSource;
        
        _updateClient = UpdateClient.Builder()
            .WithLogger((format, fmtArgs, caller) =>
            {
                Info(
                    LogSource.Service, 
                    fmtArgs.Length is 0 ? format : format.Format(fmtArgs),
                    InvocationInfo.CurrentMember(caller)
                    );
            });
    }
    
    public async ValueTask<Exception> CreateUserAsync(string username, string email, string name = null)
    {
        if (name == null)
            name = username.Capitalize();
        
        try
        {
            await Client.Users.CreateAsync(new UserUpsert
            {
                Name = name,
                Username = username,
                Email = email,
                ResetPassword = true
            });
        }
        catch (Exception e)
        {
            return e;
        }

        return null;
    }
    
    public Task<GitLabReleaseJsonResponse> GetLatestStableAsync()
    {
        try
        {
            return GitLabApi.GetLatestReleaseAsync(_http, _releaseChannels["stable"].Id);
        }
        catch
        {
            return Task.FromResult<GitLabReleaseJsonResponse>(null);
        }
    }

    public Task<GitLabReleaseJsonResponse> GetLatestCanaryAsync()
    {
        try
        {
            return GitLabApi.GetLatestReleaseAsync(_http, _releaseChannels["canary"].Id);
        }
        catch
        {
            return Task.FromResult<GitLabReleaseJsonResponse>(null);
        }
    }

    public static string GetWikiPageUrl(WikiPage page) => $"{Config.GitLabAuth.InstanceUrl}/ryubing/ryujinx/-/wikis/{page.Slug}";

    public IEnumerable<WikiPage> SearchWikiPages(string searchTerm) =>
        _cachedPages.Where(x => 
            x.Slug.ContainsIgnoreCase(searchTerm) || 
            x.Title.ContainsIgnoreCase(searchTerm)
        );

    public Gommon.Optional<WikiPage> FindPage(Func<WikiPage, bool> predicate)
        => _cachedPages.FindFirst(predicate);
}
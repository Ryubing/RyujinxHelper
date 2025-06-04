using NGitLab;
using NGitLab.Models;

namespace RyuBot.Services;

public class GitLabService : BotService
{
    private readonly PeriodicTimer _gitlabRefreshTimer = new(12.Hours());
    private readonly CancellationTokenSource _cts;
    private readonly HttpClient _http;
    
    private IWikiClient _ryubingWikiClient;

    private GitLabReleaseChannels _releaseChannels;

    private WikiPage[] _cachedPages;
    
    public GitLabClient Client { get; } = new(Config.GitLabAuth.InstanceUrl, Config.GitLabAuth.AccessToken);

    public void Init()
    {
        _ryubingWikiClient = Client.GetWikiClient(new ProjectId("ryubing/ryujinx"));

        _cachedPages = _ryubingWikiClient.All.Where(x => x.Slug != "Dumping" && x.Title != "home").ToArray();
        
        Info(LogSource.Service, $"Cached {_cachedPages.Length} wiki pages.");
        
        ExecuteBackgroundAsync(async () =>
        {
            _releaseChannels = await GitLabReleaseChannels.GetAsync(_http);
            while (await _gitlabRefreshTimer.WaitForNextTickAsync(_cts.Token))
            {
                Info(LogSource.Service, "Refreshing GitLab release channels.");
                _releaseChannels = await GitLabReleaseChannels.GetAsync(_http);
            }
        });
    } 

    public GitLabService(HttpClient httpClient, CancellationTokenSource cancellationTokenSource)
    {
        _http = httpClient;
        _cts = cancellationTokenSource;
    }

    public WikiPage[] GetWikiPages()
    {
        return _cachedPages;
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
            return _releaseChannels.Stable.GetLatestReleaseAsync(_http);
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
            return _releaseChannels.Canary.GetLatestReleaseAsync(_http);
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
using NGitLab;
using NGitLab.Models;

namespace RyuBot.Services;

public class GitLabService : BotService
{
    private IWikiClient _ryubingWikiClient;

    private WikiPage[] _cachedPages;
    
    public GitLabClient Client { get; } = new(Config.GitLabAuth.InstanceUrl, Config.GitLabAuth.AccessToken);

    public void Init()
    {
        _ryubingWikiClient = Client.GetWikiClient(new ProjectId("ryubing/ryujinx"));

        _cachedPages = _ryubingWikiClient.All.Where(x => x.Slug != "Dumping" && x.Title != "home").ToArray();
        
        Info(LogSource.Service, $"Cached {_cachedPages.Length} wiki pages.");
    }

    public WikiPage[] GetWikiPages()
    {
        return _cachedPages;
    }

    public IEnumerable<WikiPage> SearchWikiPages(string searchTerm) =>
        _cachedPages.Where(x => 
            x.Slug.ContainsIgnoreCase(searchTerm) || 
            x.Title.ContainsIgnoreCase(searchTerm)
        );

    public Gommon.Optional<WikiPage> FindPage(Func<WikiPage, bool> predicate)
        => _cachedPages.FindFirst(predicate);
}
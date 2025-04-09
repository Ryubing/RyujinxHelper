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
    
    public async ValueTask<(string TempPassword, Exception Exception)> CreateUserAsync(string username, string email, string name = null)
    {
        if (name == null)
            name = username.Capitalize();

        User user;
        
        try
        {
            user = await Client.Users.CreateAsync(new UserUpsert
            {
                Name = name,
                Username = username,
                Email = email,
                Password = StringUtil.RandomAlphanumeric(100) // intentional
            });
        }
        catch (Exception e)
        {
            return (string.Empty, e);
        }
        
        var temporaryPassword = StringUtil.RandomAlphanumeric(100);
        
        Client.Users.Update(user.Id, new UserUpsert
        {
            Password = temporaryPassword 
            // changing password after user creation causes gitlab to force the user to change their password upon first login
            // this is why the first password is not saved, it simply gets overwritten immediately
        });

        return (temporaryPassword, null);
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
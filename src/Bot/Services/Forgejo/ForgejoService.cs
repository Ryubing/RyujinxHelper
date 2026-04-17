using ForgejoApiClient;
using ForgejoApiClient.Api;
using Ryujinx.Systems.Update.Client;
using Ryujinx.Systems.Update.Common;

namespace RyuBot.Services;

public class ForgejoService : BotService
{
    private readonly PeriodicTimer _rcRefreshTimer = new(12.Hours());
    private readonly CancellationTokenSource _cts;
    private readonly UpdateClient _updateClient;

    private CacheSourceMapping _releaseChannels;

    private WikiPageMetaData[] _cachedPages;
    public WikiPageMetaData[] WikiPages => _cachedPages ?? [];

    public ForgejoClient Client { get; }

    public async Task InitAsync()
    {
        if (Client != null)
        {
            _cachedPages = await Client.Repository.ListWikiPagesAsync("projects", "Ryubing");

            Info(LogSource.Service, $"Cached {_cachedPages.Length} wiki pages.");
        }

        _releaseChannels = await _updateClient.QueryCacheSourcesAsync();
        while (await _rcRefreshTimer.WaitForNextTickAsync(_cts.Token))
        {
            Info(LogSource.Service, "Refreshing Forgejo release channels.");
            _releaseChannels = await _updateClient.QueryCacheSourcesAsync();
        }
    } 

    public ForgejoService(CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            Client = new(new Uri(Config.ForgejoAuth.InstanceUrl), Config.ForgejoAuth.AccessToken);
        }
        catch (NullReferenceException)
        {
            Error(LogSource.Service, "Forgejo auth info not found; not initializing ForgejoClient");
        }
        
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
        try
        {
            await Client.Admin.CreateUserAsync(new CreateUserOption(email: email, username: username, full_name: name,
                send_notify: true, must_change_password: true,
                password: StringUtil.RandomAlphanumeric(32)
                ));
        }
        catch (Exception e)
        {
            return e;
        }

        return null;
    }

    public Task<Release> GetLatestStableAsync()
    {
        try
        {
            return Client.Repository.GetReleaseLatestAsync(_releaseChannels.Stable.Owner, _releaseChannels.Stable.Project);
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
            return Client.Repository.GetReleaseLatestAsync(_releaseChannels.Canary!.Owner, _releaseChannels.Canary.Project);
        }
        catch
        {
            return Task.FromResult<Release>(null);
        }
    }

    public IEnumerable<WikiPageMetaData> SearchWikiPages(string searchTerm) =>
        _cachedPages.Where(x => 
            x.title.ContainsIgnoreCase(searchTerm) || 
            x.sub_url.ContainsIgnoreCase(searchTerm)
        );

    public Gommon.Optional<WikiPageMetaData> FindPage(Func<WikiPageMetaData, bool> predicate)
        => _cachedPages.FindFirst(predicate);
}
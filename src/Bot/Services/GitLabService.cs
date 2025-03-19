using NGitLab;

namespace RyuBot.Services;

public class GitLabService : BotService
{
    public GitLabClient Client { get; } = new(Config.GitLabAuth.InstanceUrl, Config.GitLabAuth.AccessToken);
}
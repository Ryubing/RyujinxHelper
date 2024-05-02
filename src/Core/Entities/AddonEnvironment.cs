// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Volte.Core.Entities;

public class AddonEnvironment(IServiceProvider provider)
{
    public IServiceProvider Services { get; } = provider;
    public DiscordSocketClient Client { get; } = provider.Get<DiscordSocketClient>();
    public CommandService Commands { get; } = provider.Get<CommandService>();
    public DatabaseService Database { get; } = provider.Get<DatabaseService>();

    public bool IsCommand(SocketUserMessage message, ulong guildId) 
        => CommandUtilities.HasAnyPrefix(message.Content, 
            new[] { Database.GetData(guildId).Configuration.CommandPrefix, $"<@{Client.CurrentUser.Id}> ", $"<@!{Client.CurrentUser.Id}> " }, 
            StringComparison.OrdinalIgnoreCase, out _, out _);

}
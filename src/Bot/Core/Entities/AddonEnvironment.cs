// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

using RyuBot.Services;

namespace RyuBot.Entities;

public class AddonEnvironment
{
    public AddonEnvironment(IServiceProvider provider)
    {
        Services = provider;
        Client = Services.Get<DiscordSocketClient>();
        Commands = Services.Get<CommandService>();
    }
    
    public IServiceProvider Services { get; }
    public DiscordSocketClient Client { get; } 
    public CommandService Commands { get; }

    public bool IsCommand(SocketUserMessage message, ulong guildId, out string targetCommand) 
        => CommandUtilities.HasAnyPrefix(message.Content, 
            new[] { Config.CommandPrefix, $"<@{Client.CurrentUser.Id}> ", $"<@!{Client.CurrentUser.Id}> " }, 
            StringComparison.OrdinalIgnoreCase, out _, out targetCommand);

}
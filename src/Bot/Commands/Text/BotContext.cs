using Qmmands;

namespace RyuBot.Commands.Text;

public class BotContext : CommandContext
{
    // ReSharper disable once SuggestBaseTypeForParameter
    public BotContext(SocketMessage msg, IServiceProvider provider) : base(provider)
    {
        Client = provider.Get<DiscordSocketClient>();
        Guild = msg.Channel.Cast<SocketTextChannel>()?.Guild;
        Channel = msg.Channel.Cast<SocketTextChannel>();
        User = msg.Author.Cast<SocketGuildUser>();
        Message = msg.Cast<SocketUserMessage>();
        Now = DateTime.Now;
    }


    public DiscordSocketClient Client { get; }
    public SocketGuild Guild { get; }
    public SocketTextChannel Channel { get; }
    public SocketGuildUser User { get; }
    public SocketUserMessage Message { get; }
    public DateTime Now { get; }
        
    public Embed CreateEmbed(StringBuilder content) => CreateEmbed(content.ToString());

    public Embed CreateEmbed(Action<EmbedBuilder> action) 
        => CreateEmbedBuilder().Apply(action).Build();
        
    public Embed CreateEmbed(string content) => CreateEmbedBuilder(content).Build();
    
    public EmbedBuilder CreateEmbedBuilder(string content = null) => new EmbedBuilder()
        .WithColor(User.GetHighestRole()?.Color ?? new Color(Config.SuccessColor))
        .WithAuthor(User.ToString(), User.GetEffectiveAvatarUrl())
        .WithDescription(content ?? string.Empty);

    public EmbedBuilder CreateEmbedBuilder(StringBuilder content) => CreateEmbedBuilder(content.ToString());
}
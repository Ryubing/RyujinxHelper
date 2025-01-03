using RyuBot.Commands.Text;
using RyuBot.Helpers;

namespace RyuBot.Interactive;

public class DeleteMessageReactionCallback : IReactionCallback
{
    public RunMode RunMode { get; } = RunMode.Parallel;
    public ICriterion<SocketReaction> Criterion { get; } = new EnsureReactionFromSourceUserCriterion();
    public TimeSpan? Timeout { get; } = 10.Seconds();
    public RyujinxBotContext Context { get; }
    public RestUserMessage Message { get; private set; }
    public async ValueTask<bool> HandleAsync(SocketReaction reaction)
    {
        if (reaction.Emote.Name.EqualsIgnoreCase(Emojis.X.Name))
        {
            return await reaction.Message.Value.TryDeleteAsync();
        }

        return false;

    }

    public DeleteMessageReactionCallback(RyujinxBotContext ctx, Embed embed)
    {
        Context = ctx;
        _ = Task.Run(async () => await (Message = await Context.Channel.SendMessageAsync(embed: embed)).AddReactionAsync(Emojis.X));
    }
}
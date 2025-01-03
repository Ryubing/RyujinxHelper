using RyuBot.Commands.Text;

namespace RyuBot.Interactive;

public interface IReactionCallback
{
    RunMode RunMode { get; }
    ICriterion<SocketReaction> Criterion { get; }
    RyujinxBotContext Context { get; }

    ValueTask<bool> HandleAsync(SocketReaction reaction);
}
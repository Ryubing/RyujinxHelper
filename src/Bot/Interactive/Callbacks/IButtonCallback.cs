using Discord.Interactions;
using RyuBot.Commands.Text;
using RunMode = Qmmands.RunMode;

namespace RyuBot.Interactive;

public interface IButtonCallback
{
    VolteContext MessageContext { get; }
    IUserMessage PagerMessage { get; }
    RunMode RunMode { get; }
    ICriterion<SocketInteractionContext<SocketMessageComponent>> Criterion { get; }

    ValueTask<bool> HandleAsync(SocketInteractionContext<SocketMessageComponent> buttonContext);
}
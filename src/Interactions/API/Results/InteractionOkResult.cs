using Discord.Interactions;

namespace Volte.Interactions.Results;

public class InteractionOkResult<TInteraction> : RuntimeResult where TInteraction : SocketInteraction
{
    public InteractionOkResult() : base(null, string.Empty)
    {
    }

    public ReplyBuilder<TInteraction> Reply;
}


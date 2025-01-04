using Discord.Interactions;

namespace RyuBot.Interactions.Results;

public class OkResult<TInteraction> : RuntimeResult where TInteraction : SocketInteraction
{
    public OkResult(ReplyBuilder<TInteraction> reply) : base(null, string.Empty)
    {
        Reply = reply;
    }

    public readonly ReplyBuilder<TInteraction> Reply;
    
    public static implicit operator Task<OkResult<TInteraction>>(OkResult<TInteraction> input) 
        => Task.FromResult(input);
}


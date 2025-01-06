using Discord.Interactions;

namespace RyuBot.Interactions.Results;

public class BadRequestResult<TInteraction> : BotResultBase where TInteraction : SocketInteraction
{
    public SocketInteractionContext<TInteraction> Context { get; }
    
    public bool DidDefer { get; }

    public BadRequestResult(SocketInteractionContext<TInteraction> context, string error, bool didDefer) 
        : base(null, error)
    {
        Context = context;
        DidDefer = didDefer;
    }

    public override Task ExecuteAsync() =>
        Context.CreateReplyBuilder(true)
            .WithDeferral(DidDefer)
            .WithEmbed(e =>
                e.WithTitle("No can do, partner.")
                    .WithDescription(ErrorReason)
                    .WithColor(Color.Red)
                    .WithCurrentTimestamp()
            ).ExecuteAsync();
}
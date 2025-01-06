namespace RyuBot.Interactions.Results;

public class OkResult<TInteraction> : BotResultBase where TInteraction : SocketInteraction
{
    public OkResult(ReplyBuilder<TInteraction> reply)
    {
        Reply = reply;
    }

    public readonly ReplyBuilder<TInteraction> Reply;

    public override Task ExecuteAsync() => Reply.ExecuteAsync();
}


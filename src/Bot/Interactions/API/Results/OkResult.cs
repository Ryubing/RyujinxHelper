namespace RyuBot.Interactions.Results;

public class OkResult<TInteraction> : BotResultBase where TInteraction : SocketInteraction
{
    public OkResult(ReplyBuilder<TInteraction> reply)
    {
        _reply = reply;
    }
    
    public OkResult(ReplyBuilder<TInteraction> reply, Func<Task> afterCompletion, bool awaitCallback = true) : this(reply)
    {
        _afterCompletion = afterCompletion;
        _awaitCompletionCallback = awaitCallback;
    }

    private readonly ReplyBuilder<TInteraction> _reply;

    private readonly bool _awaitCompletionCallback;

    private readonly Func<Task> _afterCompletion;

    public override async Task ExecuteAsync()
    {
        await _reply.ExecuteAsync();

        if (_afterCompletion is null) return;

        if (_awaitCompletionCallback)
            await _afterCompletion();
        else
            ExecuteBackgroundAsync(_afterCompletion);
    }
}


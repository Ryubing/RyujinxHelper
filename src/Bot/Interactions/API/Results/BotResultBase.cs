using Discord.Interactions;

namespace RyuBot.Interactions.Results;

public abstract class BotResultBase(InteractionCommandError? error, string reason) : RuntimeResult(error, reason)
{
    protected BotResultBase() : this(null, string.Empty) { }
    
    public virtual Task ExecuteAsync() => Task.CompletedTask;
    
    public static implicit operator Task<RuntimeResult>(BotResultBase input) 
        => Task.FromResult<RuntimeResult>(input);
}
using Discord.Interactions;
using RyuBot.Interactions.Results;

namespace RyuBot.Interactions;

[Obsolete("Use an inheritor of this class; not this class directly.")]
public abstract class RyujinxBotModuleBase<TInteraction> : InteractionModuleBase<SocketInteractionContext<TInteraction>> 
    where TInteraction : SocketInteraction
{
    public RyujinxBotInteractionService Interactions { get; set; }
    
    private bool DidDefer { get; set; }

    protected new async Task DeferAsync(bool ephemeral = false, RequestOptions options = null)
    {
        await Context.Interaction.DeferAsync(ephemeral, options);
        DidDefer = true;
    }

    protected ReplyBuilder<TInteraction> CreateReplyBuilder(
        bool ephemeral = false
    ) => Context.CreateReplyBuilder(ephemeral, DidDefer);
    
    public bool IsInGuild() => Context.Guild != null;
    
    protected NoneResult None() => new();

    protected BadRequestResult<TInteraction> BadRequest(string reason) => new(Context, reason, DidDefer);

    protected OkResult<TInteraction> Ok(ReplyBuilder<TInteraction> reply) => new(reply);

    protected OkResult<TInteraction> Ok(string message, bool ephemeral = false) 
        => Ok(CreateReplyBuilder(ephemeral).WithEmbedFrom(message));
    
    protected OkResult<TInteraction> Ok(EmbedBuilder embed, bool ephemeral = false) 
        => new(CreateReplyBuilder(ephemeral).WithEmbeds(embed));
    
    protected OkResult<TInteraction> Ok(
        ReplyBuilder<TInteraction> reply, 
        Func<Task> afterCompletion, 
        bool awaitCallback = true
    ) => new(reply, afterCompletion, awaitCallback);

    protected OkResult<TInteraction> Ok(string message, Func<Task> afterCompletion, bool awaitCallback = true, bool ephemeral = false)
        => Ok(CreateReplyBuilder(ephemeral).WithEmbedFrom(message), afterCompletion, awaitCallback);
    
    protected OkResult<TInteraction> Ok(EmbedBuilder embed, Func<Task> afterCompletion, bool awaitCallback = true, bool ephemeral = false) 
        => new(CreateReplyBuilder(ephemeral).WithEmbeds(embed), afterCompletion, awaitCallback);
}

#pragma warning disable CS0618 // Type or member is obsolete
public abstract class RyujinxBotSlashCommandModule : RyujinxBotModuleBase<SocketSlashCommand>;
public abstract class RyujinxBotMessageCommandModule : RyujinxBotModuleBase<SocketMessageCommand>;
public abstract class RyujinxBotUserCommandModule : RyujinxBotModuleBase<SocketUserCommand>;
public abstract class RyujinxBotMessageComponentModule : RyujinxBotModuleBase<SocketMessageComponent>;
public abstract class RyujinxBotModalModule : RyujinxBotModuleBase<SocketModal>;
#pragma warning restore CS0618 // Type or member is obsolete
using Discord.Interactions;
using RyuBot.Entities;
using RyuBot.Interactions;
using RyuBot.Interactions.Results;

namespace RyuBot.Interactions;

[Obsolete("Use an inheritor of this class; not this class directly.")]
public abstract class RyujinxBotModuleBase<T> : InteractionModuleBase<SocketInteractionContext<T>> where T : SocketInteraction
{
    public RyujinxBotInteractionService Interactions { get; set; }
    
    private bool DidDefer { get; set; }

    protected new async Task DeferAsync(bool ephemeral = false, RequestOptions options = null)
    {
        await Context.Interaction.DeferAsync(ephemeral, options);
        DidDefer = true;
    }

    protected ReplyBuilder<T> CreateReplyBuilder(
        bool ephemeral = false
    ) => Context.CreateReplyBuilder(ephemeral, DidDefer);
    
    public bool IsInGuild() => Context.Guild != null;
    
    protected NoneResult None() => new();

    protected BadRequestResult BadRequest(string reason) => new(reason, DidDefer);

    protected OkResult<T> Ok(ReplyBuilder<T> reply) => new(reply);

    protected OkResult<T> Ok(string message, bool ephemeral = false) 
        => Ok(CreateReplyBuilder(ephemeral).WithEmbedFrom(message));
    
    protected OkResult<T> Ok(EmbedBuilder embed, bool ephemeral = false) 
        => new(CreateReplyBuilder(ephemeral).WithEmbeds(embed));
}

#pragma warning disable CS0618 // Type or member is obsolete
public abstract class RyujinxBotSlashCommandModule : RyujinxBotModuleBase<SocketSlashCommand>;
public abstract class RyujinxBotMessageCommandModule : RyujinxBotModuleBase<SocketMessageCommand>;
public abstract class RyujinxBotUserCommandModule : RyujinxBotModuleBase<SocketUserCommand>;
public abstract class RyujinxBotMessageComponentModule : RyujinxBotModuleBase<SocketMessageComponent>;
public abstract class RyujinxBotModalModule : RyujinxBotModuleBase<SocketModal>;
#pragma warning restore CS0618 // Type or member is obsolete
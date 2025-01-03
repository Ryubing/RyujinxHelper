using Discord.Interactions;
using RyuBot.Entities;
using RyuBot.Interactions.Results;
using RyuBot.Services;

namespace RyuBot.Interactions.Commands;

public abstract class RyujinxBotInteractionModule<T> : InteractionModuleBase<SocketInteractionContext<T>> where T : SocketInteraction
{
    public bool IsInGuild() => Context.Guild != null;

    protected InteractionBadRequestResult BadRequest(string reason) => new(reason);

    protected InteractionOkResult<T> Ok(ReplyBuilder<T> reply) => new(reply);

    protected InteractionOkResult<T> Ok(string message, bool ephemeral = false) 
        => Ok(Context.CreateReplyBuilder(ephemeral).WithEmbedFrom(message));
    
    protected InteractionOkResult<T> Ok(EmbedBuilder embed, bool ephemeral = false) 
        => new(Context.CreateReplyBuilder(ephemeral).WithEmbeds(embed));
}

public abstract class RyujinxBotSlashCommandModule : RyujinxBotInteractionModule<SocketSlashCommand>;
public abstract class RyujinxBotMessageCommandModule : RyujinxBotInteractionModule<SocketMessageCommand>;
public abstract class RyujinxBotUserCommandModule : RyujinxBotInteractionModule<SocketUserCommand>;
public abstract class RyujinxBotMessageComponentModule : RyujinxBotInteractionModule<SocketMessageComponent>;
public abstract class RyujinxBotModalModule : RyujinxBotInteractionModule<SocketModal>;
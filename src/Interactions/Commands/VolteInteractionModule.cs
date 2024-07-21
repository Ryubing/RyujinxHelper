using Discord.Interactions;
using Volte.Interactions.Results;

namespace Volte.Interactions.Commands;

public class VolteInteractionModule<T> : InteractionModuleBase<SocketInteractionContext<T>> where T : SocketInteraction
{
    protected InteractionBadRequestResult BadRequest(string reason) => new(reason);

    protected InteractionOkResult<T> Ok(ReplyBuilder<T> reply) => new()
    {
        Reply = reply
    };
}

public class VolteSlashCommandModule : VolteInteractionModule<SocketSlashCommand>;
public class VolteMessageCommandModule : VolteInteractionModule<SocketMessageCommand>;
public class VolteUserCommandModule : VolteInteractionModule<SocketUserCommand>;
public class VolteMessageComponentModule : VolteInteractionModule<SocketMessageComponent>;
public class VolteModalModule : VolteInteractionModule<SocketModal>;
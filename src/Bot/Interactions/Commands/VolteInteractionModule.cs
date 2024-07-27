using Discord.Interactions;
using Volte.Interactions.Results;

namespace Volte.Interactions.Commands;

public class VolteInteractionModule<T> : InteractionModuleBase<SocketInteractionContext<T>> where T : SocketInteraction
{
    public bool IsInGuild() => Context.Guild != null;

    public GuildData GetData() 
        => VolteBot.ServiceProvider.Get<DatabaseService>().GetData(Context.Guild);
    
    public void ModifyData(DataEditor modifier)
        => VolteBot.ServiceProvider.Get<DatabaseService>().Modify(Context.Guild.Id, modifier);

    protected InteractionBadRequestResult BadRequest(string reason) => new(reason);

    protected InteractionOkResult<T> Ok(ReplyBuilder<T> reply) => new(reply);

    protected InteractionOkResult<T> Ok(string message, bool ephemeral = false) 
        => Ok(Context.CreateReplyBuilder(ephemeral).WithEmbedFrom(message));
    
    protected InteractionOkResult<T> Ok(EmbedBuilder embed, bool ephemeral = false) 
        => new(Context.CreateReplyBuilder(ephemeral).WithEmbeds(embed));
}

public class VolteSlashCommandModule : VolteInteractionModule<SocketSlashCommand>;
public class VolteMessageCommandModule : VolteInteractionModule<SocketMessageCommand>;
public class VolteUserCommandModule : VolteInteractionModule<SocketUserCommand>;
public class VolteMessageComponentModule : VolteInteractionModule<SocketMessageComponent>;
public class VolteModalModule : VolteInteractionModule<SocketModal>;
using Discord.Interactions;
using RyuBot.Services;

namespace RyuBot.Interactions.Commands;

public class RequireGuildAdminPreconditionAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(
        IInteractionContext context,
        ICommandInfo commandInfo,
        IServiceProvider services
    )
    {
        if (context.Guild is null)
            return Task.FromResult(PreconditionResult.FromError("This command can only be executed in a guild."));

        var db = services.Get<DatabaseService>();
        var data = db.GetData(context.Guild.Id);

        return Task.FromResult(
            data.Configuration.Moderation.AdminRole == context.User.Id
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("This command requires you to be an administrator.")
        );
    }
}
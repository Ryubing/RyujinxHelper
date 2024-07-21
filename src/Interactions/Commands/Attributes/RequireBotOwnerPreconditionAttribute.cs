using Discord.Interactions;

namespace Volte.Interactions.Commands;

public class RequireBotOwnerPreconditionAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(
        IInteractionContext context,
        ICommandInfo commandInfo,
        IServiceProvider services
    )
    {
        return Task.FromResult(context.User.Id == Config.Owner
            ? PreconditionResult.FromSuccess()
            : PreconditionResult.FromError("Insufficient permission.")
        );
    }
}
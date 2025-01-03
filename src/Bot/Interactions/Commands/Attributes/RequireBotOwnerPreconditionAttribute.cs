using Discord.Interactions;

namespace RyuBot.Interactions.Commands;

public class RequireBotOwnerPreconditionAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(
        IInteractionContext context,
        ICommandInfo commandInfo,
        IServiceProvider services
    ) => Task.FromResult(context.User.IsBotOwner()
        ? PreconditionResult.FromSuccess()
        : PreconditionResult.FromError("Insufficient permission.")
    );
}
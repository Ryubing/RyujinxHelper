using Discord.Interactions;

namespace RyuBot.Commands.Interactions;

public class RequireProjectMaintainerPreconditionAttribute : PreconditionAttribute
{
    public const ulong GreemDev = 168548441939509248;
    public const ulong Keaton = 394186598071140383;
    
    private static readonly ulong[] Maintainers = [ GreemDev, Keaton ];

    public override Task<PreconditionResult> CheckRequirementsAsync(
        IInteractionContext context,
        ICommandInfo commandInfo,
        IServiceProvider services) =>
        Task.FromResult(Maintainers.Contains(context.User.Id)
            ? PreconditionResult.FromSuccess()
            : PreconditionResult.FromError("Not an approved project maintainer.")
        );
}
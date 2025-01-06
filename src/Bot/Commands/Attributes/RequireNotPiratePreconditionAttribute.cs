using Discord.Interactions;

namespace RyuBot.Commands;

public class RequireNotPiratePreconditionAttribute : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckRequirementsAsync(
        IInteractionContext context, 
        ICommandInfo commandInfo, 
        IServiceProvider services)
    {
        if (!Config.WhitelistGuildPirateRoles.TryGetValue(context.Guild?.Id ?? ulong.MaxValue, out var pirateRoleId))
            return PreconditionResult.FromError("This command must be run in a guild.");
        
        if (pirateRoleId is 0) // guild does not have a configured pirate role
            return PreconditionResult.FromSuccess();

        var member = await context.Guild!.GetUserAsync(context.User.Id);
        return member.RoleIds.Contains(pirateRoleId) 
            ? PreconditionResult.FromError("You are not allowed to get help.") 
            : PreconditionResult.FromSuccess();
    }
}
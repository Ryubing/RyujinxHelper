﻿using Discord.Interactions;

namespace RyuBot.Commands.Interactions;

public class RequireRyubingGuildPreconditionAttribute : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckRequirementsAsync(
        IInteractionContext context, 
        ICommandInfo commandInfo, 
        IServiceProvider services)
    {
        if (context.Guild is null)
            return PreconditionResult.FromError("This command must be run in a guild.");
        
        return context.Guild.Id is not 1294443224030511104
            ? PreconditionResult.FromError($"This command can only be run in {Format.Url("Ryubing", "https://discord.gg/ryujinx")}.") 
            : PreconditionResult.FromSuccess();
    }
}
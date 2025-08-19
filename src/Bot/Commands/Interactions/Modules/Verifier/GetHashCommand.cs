using Discord.Interactions;

namespace RyuBot.Commands.Interactions.Modules;

public partial class VerifierModule
{
    [SlashCommand("get-hash", "Get a hash for use with the Switch Verifier Homebrew.")]
    public async Task<RuntimeResult> GetHashAsync()
    {
        if (Context.User is not SocketGuildUser member) return None();

        if (member.HasRole(VerifierService.VerifiedSwitchOwnerRoleId))
            return BadRequest("You are already verified.");

        await DeferAsync(true);

        var response = await Verifier.GetHashAsync(Context.User.Id);

        if (response.Result is not ResultCode.Success) // shouldn't happen
            return BadRequest($"{Enum.GetName(response.Result) ?? response.Result.ToString()}: Bad input on hash request.");


        return Ok(CreateReplyBuilder(true).WithContent(response.Hash.Value));
    }
}
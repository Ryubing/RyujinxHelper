using Discord.Interactions;

namespace RyuBot.Commands.Interactions.Modules;

public partial class VerifierModule
{
    [SlashCommand("get-hash", "Get a hash for use with the Switch Verifier Homebrew.")]
    [RequireRyubingGuildPrecondition]
    public async Task<RuntimeResult> GetHashAsync()
    {
        await DeferAsync(true);

        var response = await Verifier.GetHashAsync(Context.User.Id);

        if (response.Result is not ResultCode.Success) // shouldn't happen
            return BadRequest($"{Enum.GetName(response.Result) ?? response.Result.ToString()}: Bad input on hash request.");


        return Ok(CreateReplyBuilder(true).WithContent(response.Hash.Value));
    }
}
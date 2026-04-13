using Discord.Interactions;

namespace RyuBot.Commands.Interactions.Modules;

public partial class ForgejoModule
{
    [SlashCommand("create-user", "Create a user on the Ryubing Forgejo. Bot owner only.")]
    [RequireBotOwnerPrecondition]
    public async Task<RuntimeResult> CreateUserAsync(string username, string email, string name = null)
    {
        await DeferAsync(true);

        var error = await Forgejo.CreateUserAsync(username, email, name);

        if (error != null)
        {
            Error(error);
            return BadRequest(String(sb =>
            {
                sb.AppendLine("Failed to create user. Likely reason is the configured Forgejo access token does not have administrator rights.");
                sb.AppendLine(Format.Code(error.Message, string.Empty));
            }));
        }
        

        return Ok(
            CreateReplyBuilder()
                .WithEmbed(eb =>
                {
                    eb.WithTitle($"Created user '{username}'");
                    eb.WithDescription("If the provided email was valid, they will receive a verification email.");
                })
        );
    }
}
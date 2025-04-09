using Discord.Interactions;

namespace RyuBot.Commands.Interactions.Modules;

public partial class GitLabModule
{
    [SlashCommand("create-user", "Create a user on the Ryubing GitLab. Bot owner only.")]
    [RequireBotOwnerPrecondition]
    public async Task<RuntimeResult> CreateUserAsync(string username, string email, string name = null)
    {
        await DeferAsync(true);

        var (tempPassword, error) = await GitLab.CreateUserAsync(username, email, name);

        if (error != null)
        {
            Error(error);
            return BadRequest(
                "Failed to create user. Likely reason is the configured GitLab access token does not have administrator rights.");
        }
        

        return Ok(
            CreateReplyBuilder()
                .WithEmbed(eb =>
                {
                    eb.WithTitle($"Created user '{username}'");
                    eb.WithDescription(String(sb =>
                    {
                        sb.AppendLine("Copy and paste this and send it to the user:");
                        sb.Append(Format.Code(
                            string.Join('\n',
                                $"{Config.GitLabAuth.InstanceUrl}/users/sign_in", 
                                $"__Username__:" +
                                $"`{username}`", 
                                $"__Password__:" +
                                $"`{tempPassword}`", 
                                "Change password when prompted."
                                ), 
                            string.Empty));
                    }));
                })
        );
    }
}
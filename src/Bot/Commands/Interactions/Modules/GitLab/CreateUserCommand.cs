using Discord.Interactions;
using NGitLab.Models;

namespace RyuBot.Commands.Interactions.Modules;

public partial class GitLabModule
{
    [SlashCommand("create-user", "Create a user on the Ryubing GitLab. Bot owner only.")]
    [RequireBotOwnerPrecondition]
    public async Task<RuntimeResult> CreateUserAsync(string username, string email, string name = null)
    {
        if (name == null)
            name = username.Capitalize();

        await DeferAsync(true);
        
        User user;
        
        try
        {
            user = await GitLab.Client.Users.CreateAsync(new UserUpsert
            {
                Name = name,
                Username = username,
                Email = email,
                Password = StringUtil.RandomAlphanumeric(100) // intentional
            });
        }
        catch (Exception e)
        {
            Error(e);
            return None();
        }
        
        var temporaryPassword = StringUtil.RandomAlphanumeric(100);

        GitLab.Client.Users.Update(user.Id, new UserUpsert
        {
            Password = temporaryPassword 
            // changing password after user creation causes gitlab to force the user to change their password upon first login
            // this is why the first password is not saved, it simply gets overwritten immediately
        });

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
                                $"Username: `{username}`", 
                                $"Password: `{temporaryPassword}`", 
                                "Change password when prompted."
                                ), 
                            string.Empty));
                    }));
                })
        );
    }
}
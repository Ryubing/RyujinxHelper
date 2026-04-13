using Discord.Interactions;
using RyuBot.Interactions;

namespace RyuBot.Commands.Interactions.Modules;

[Group("forgejo", "Forgejo access & administration commands.")]
public partial class ForgejoModule : RyujinxBotSlashCommandModule
{
    public ForgejoService Forgejo { get; set; }
    
    // :Angryreaction:
    public GitHubService GitHub { get; set; }
}
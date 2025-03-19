using Discord.Interactions;
using RyuBot.Interactions;

namespace RyuBot.Commands.Interactions.Modules;

[Group("gitlab", "Gitlab access & administration commands.")]
public partial class GitLabModule : RyujinxBotSlashCommandModule
{
    public GitLabService GitLab { get; set; }
}
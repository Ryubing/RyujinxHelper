using Discord.Interactions;
using RyuBot.Commands;
using RyuBot.Interactions;

namespace Volte.Commands.Modules;

#if DEBUG

public class BotOwnerModule : RyujinxBotSlashCommandModule
{
    [SlashCommand("cc", "Clears the commands of the bot. Bot owner only.")]
    [RequireBotOwnerPrecondition]
    public async Task<RuntimeResult> ClearCommandsAsync()
    {
        await Interactions.ClearCommandsAsync();
        return Ok("Cleared commands.", true);
    }
}

#endif
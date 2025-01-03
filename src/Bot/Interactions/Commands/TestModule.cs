#if DEBUG

using Discord.Interactions;

namespace RyuBot.Interactions.Commands;

public class TestModule : VolteSlashCommandModule
{
    [SlashCommand("test", "slash command api test")]
    public async Task<RuntimeResult> TestAsync(string parameter1)
    {
        return BadRequest($"Bruh {parameter1}");
    }
}

#endif
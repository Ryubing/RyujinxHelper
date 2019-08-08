using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Qmmands;
using Volte.Commands.Checks;
using Volte.Commands.Results;

namespace Volte.Commands.Modules
{
    public sealed partial class AdminModule : VolteModule
    {
        [Command("ModLog")]
        [Description("Sets the channel to be used for mod log.")]
        [Remarks("Usage: |prefix|modlog {channel}")]
        [RequireGuildAdmin]
        public Task<ActionResult> ModLogAsync(SocketTextChannel c)
        {
            Context.GuildData.Configuration.Moderation.ModActionLogChannel = c.Id;
            Db.UpdateData(Context.GuildData);
            return Ok($"Set {c.Mention} as the channel to be used by mod log.");
        }
    }
}
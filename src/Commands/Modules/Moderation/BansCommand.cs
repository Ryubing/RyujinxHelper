using System.Linq;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using Volte.Core.Entities;
using Gommon;

namespace Volte.Commands.Modules
{
    public sealed partial class ModerationModule
    {
        [Command("Bans")]
        [Description("Shows all bans in this guild.")]
        public async Task<ActionResult> BansAsync()
        {
            var banList = (await Context.Guild.GetBansAsync().FlattenAsync()).ToList();
            return banList.Any()
                ? Ok(banList.Select(b => $"**{b.User}**: {Format.Code(b.Reason ?? "No reason provided.")}").JoinToString('\n'))
                : BadRequest("This guild doesn't have anyone banned.");
        }
    }
}
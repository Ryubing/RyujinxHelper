using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Qmmands;
using Volte.Core.Entities;

namespace Volte.Commands.Modules
{
    [Group("Starboard", "Sb")]
    [RequireGuildAdmin]
    public sealed class StarboardModule : VolteModule
    {
        [Command("Channel", "Ch")]
        [Description("Sets the channel to be used by starboard when a message is starred.")]
        public Task<ActionResult> StarboardChannelAsync(SocketChannel channel)
        {
            Context.Modify(data =>
            {
                data.Configuration.Starboard.StarboardChannel = channel.Id;
            });
            return Ok($"Successfully set the starboard channel to {MentionUtils.MentionChannel(channel.Id)}.");
        }

        [Command("Amount", "Count")]
        [Description("Sets the amount of stars required on a message for it to be posted to the Starboard.")]
        public Task<ActionResult> StarsRequiredToPostAsync(int amount)
        {
            Context.Modify(data =>
            {
                data.Configuration.Starboard.StarsRequiredToPost = amount;
            });
            return Ok($"Set the amount of stars required to be posted as a starboard message to **{amount}**.");
        }

        [Command("Enable")]
        [Description("Enable or disable the Starboard in this guild.")]
        public Task<ActionResult> StarboardEnableAsync(bool enabled)
        {
            Context.Modify(data =>
            {
                data.Configuration.Starboard.Enabled = enabled;
            });
            return Ok(
                enabled ? "Enabled the Starboard in this Guild." : "Disabled the Starboard in this Guild.");
        }
    }
}
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Volte.Core;
using Volte.Core.Entities;
using Volte.Core.Helpers;

namespace Volte.Services
{
    public sealed class GuildService : IVolteService
    {
        private readonly DiscordShardedClient _client;

        public GuildService(DiscordShardedClient discordShardedClient) 
            => _client = discordShardedClient;

        public async Task OnJoinAsync(JoinedGuildEventArgs args)
        {
            Logger.Debug(LogSource.Volte, "Joined a guild.");
            if (Config.BlacklistedOwners.Contains(args.Guild.Owner.Id))
            {
                Logger.Warn(LogSource.Volte,
                    $"Left guild \"{args.Guild.Name}\" owned by blacklisted owner {args.Guild.Owner}.");
                await args.Guild.LeaveAsync();
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Hey there!")
                .WithAuthor(await _client.Rest.GetUserAsync(Config.Owner))
                .WithColor(Config.SuccessColor)
                .WithDescription("Thanks for inviting me! Here's some basic instructions on how to set me up.")
                .AddField("Set your staff roles", "$setup", true)
                .AddField("Permissions", new StringBuilder()
                    .AppendLine("It is recommended to give me the Administrator permission to avoid any permission errors that may happen.")
                    .AppendLine("You *can* get away with just send messages, ban members, kick members, and the like if you don't want to give me admin; however")
                    .AppendLine("if you're wondering why you're getting permission errors, that's *probably* why.")
                    .ToString())
                .AddField("Support Server", "https://discord.gg/H8bcFr2");

            Logger.Debug(LogSource.Volte,
                "Attempting to send the guild owner the introduction message.");
            try
            {
                await embed.SendToAsync(args.Guild.Owner);
                Logger.Error(LogSource.Volte,
                    "Sent the guild owner the introduction message.");
            }
            catch (Exception)
            {
                var c = args.Guild.TextChannels.MaxBy(x => x.Position);
                Logger.Error(LogSource.Volte,
                    "Could not DM the guild owner; sending to the upper-most channel instead.");
                if (c != null) await embed.SendToAsync(c);
            }
            
        }
    }
}
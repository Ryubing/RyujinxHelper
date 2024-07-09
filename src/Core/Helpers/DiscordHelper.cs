using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Gommon;
using Humanizer;
using Volte.Commands.Text;
using Volte.Core.Entities;
using Volte.Services;

namespace Volte.Core.Helpers
{
    public static class DiscordHelper
    {
        public static string Zws => "\u200B";
        public static string Wave => "\uD83D\uDC4B";
        public static string X => "\u274C";
        public static string BallotBoxWithCheck => "\u2611";
        public static string Clap => "\uD83D\uDC4F";
        public static string OkHand => "\uD83D\uDC4C";
        public static string One => "1️⃣";
        public static string Two => "2️⃣";
        public static string Three => "3️⃣";
        public static string Four => "4️⃣";
        public static string Five => "5️⃣";
        public static string Six => "6️⃣";
        public static string Seven => "7️⃣";
        public static string Eight => "8️⃣";
        public static string Nine => "9️⃣";
        public static string First => "⏮";
        public static string Left => "◀";
        public static string Right => "▶";
        public static string Last => "⏭";
        public static string WhiteSquare => "⏹";
        public static string OctagonalSign => "🛑";
        public static string E1234 => "🔢";
        public static string Question => "\u2753";
        public static string Star => "\u2B50";

        public static List<Emoji> GetPollEmojis()
            => [
                One.ToEmoji(), Two.ToEmoji(), Three.ToEmoji(), Four.ToEmoji(), Five.ToEmoji(),
                Six.ToEmoji(), Seven.ToEmoji(), Eight.ToEmoji(), Nine.ToEmoji()
            ];

        public static RequestOptions CreateRequestOptions(Action<RequestOptions> initializer) 
            => new RequestOptions().Apply(initializer);


        /// <summary>
        ///     Checks if the current user is the user identified in the bot's config.
        /// </summary>
        /// <param name="user">The current user</param>
        /// <returns>True, if the current user is the bot's owner; false otherwise.</returns>
        public static bool IsBotOwner(this SocketGuildUser user)
            => Config.Owner == user.Id;

        private static bool IsGuildOwner(this SocketGuildUser user)
            => user.Guild.OwnerId == user.Id || IsBotOwner(user);

        public static bool IsModerator(this VolteContext ctx, SocketGuildUser user)
            => user.HasRole(ctx.GuildData.Configuration.Moderation.ModRole) 
               || ctx.IsAdmin(user) 
               || IsGuildOwner(user);

        public static bool HasRole(this SocketGuildUser user, ulong roleId)
            => user.Roles.Select(x => x.Id).Contains(roleId);

        public static bool IsAdmin(this VolteContext ctx, SocketGuildUser user)
            => HasRole(user, ctx.GuildData.Configuration.Moderation.AdminRole) 
               || IsGuildOwner(user);

        public static async Task<bool> TrySendMessageAsync(this SocketGuildUser user, string text = null,
            bool isTts = false, Embed embed = null, RequestOptions options = null)
        {
            try
            {
                await user.SendMessageAsync(text, isTts, embed, options);
                return true;
            }
            catch (HttpException)
            {
                return false;
            }
        }

        public static SocketRole GetHighestRole(this SocketGuildUser member, bool requireColor = true)
            => member.Roles.Where(x => !requireColor || x.HasColor()).MaxBy(x => x.Position);

        public static bool TryGetSpotifyStatus(this IGuildUser user, out SpotifyGame spotify)
        {
            spotify = user.Activities.FirstOrDefault(x => x is SpotifyGame).Cast<SpotifyGame>();
            return spotify != null;
        }
        
        public static string ToMarkdownTimestamp(long unixSeconds, char timestampType)
            => $"<t:{unixSeconds}:{timestampType}>";
        
        public static string ToDiscordTimestamp(this DateTimeOffset dto, TimestampType type) =>
            ToMarkdownTimestamp(dto.ToUnixTimeSeconds(), (char)type);

        public static string ToDiscordTimestamp(this DateTime date, TimestampType type) => 
            new DateTimeOffset(date).ToDiscordTimestamp(type);

        public static async Task<bool> TrySendMessageAsync(this SocketTextChannel channel, string text = null,
            bool isTts = false, Embed embed = null, RequestOptions options = null)
        {
            try
            {
                await channel.SendMessageAsync(text, isTts, embed, options);
                return true;
            }
            catch (HttpException)
            {
                return false;
            }
        }

        public static string GetInviteUrl(this IDiscordClient client, bool withAdmin = true)
            => client.GetInviteUrl(withAdmin ? 8 : 402992246);
        
        public static string GetInviteUrl(this IDiscordClient client, long permissions)
            => $"https://discord.com/oauth2/authorize?client_id={client.CurrentUser.Id}&permissions={permissions}&scope=bot%20applications.commands";

        public static SocketUser GetOwner(this BaseSocketClient client)
            => client.GetUser(Config.Owner);

        public static SocketGuild GetPrimaryGuild(this BaseSocketClient client)
            => client.GetGuild(405806471578648588); // TODO: config option

        private static readonly string[] _ignoredLogMessages =
        [
            "You're using the GuildPresences intent without listening to the PresenceUpdate event",
            "application_command",
            "unknown dispatch"
        ];
        
        public static async Task RegisterVolteEventHandlersAsync(this DiscordSocketClient client, ServiceProvider provider)
        {
            var welcome = provider.Get<WelcomeService>();
            var autorole = provider.Get<AutoroleService>();
            var mod = provider.Get<ModerationService>();
            var starboard = provider.Get<StarboardService>();
            var msgService = provider.Get<MessageService>();
            
            client.MessageReceived += async socketMessage =>
            {
                if (socketMessage.ShouldHandle(out var msg))
                {
                    if (msg.Channel is IDMChannel dm)
                        await dm.SendMessageAsync("Currently, I do not support commands via DM.");
                    else
                        await msgService.HandleMessageAsync(new MessageReceivedEventArgs(socketMessage, provider));
                }
            };
            
            client.Log += async m =>
            {
                if (!m.Message.ContainsAnyIgnoreCase(_ignoredLogMessages))
                    await Task.Run(() => HandleLogEvent(new LogEventArgs(m)));
            };

            client.Ready += async () =>
            {
                var guilds = client.Guilds.Count;
                var users = client.Guilds.SelectMany(x => x.Users).DistinctBy(x => x.Id).Count();
                var channels = client.Guilds.SelectMany(x => x.Channels).DistinctBy(x => x.Id).Count();

                PrintHeader();
                Info(LogSource.Volte, "Use this URL to invite me to your guilds:");
                Info(LogSource.Volte, $"{client.GetInviteUrl()}");
                Info(LogSource.Volte, $"Logged in as {client.CurrentUser.Username}#{client.CurrentUser.Discriminator}");
                Info(LogSource.Volte, $"Default command prefix is: \"{Config.CommandPrefix}\"");
                Info(LogSource.Volte, "Connected to:");
                Info(LogSource.Volte, $"     {"guild".ToQuantity(guilds)}");
                Info(LogSource.Volte, $"     {"user".ToQuantity(users)}");
                Info(LogSource.Volte, $"     {"channel".ToQuantity(channels)}");

                var (type, name, streamer) = Config.ParseActivity();

                if (streamer is null && type != ActivityType.CustomStatus)
                {
                    await client.SetGameAsync(name, null, type);
                    Info(LogSource.Volte, $"Set {client.CurrentUser.Username}'s game to \"{Config.Game}\".");
                }
                else if (type != ActivityType.CustomStatus)
                {
                    await client.SetGameAsync(name, Config.FormattedStreamUrl, type);
                    Info(LogSource.Volte,
                        $"Set {client.CurrentUser.Username}'s activity to \"{type}: {name}\", at Twitch user {Config.Streamer}.");
                }

                Executor.ExecuteBackgroundAsync(async () =>
                {
                    foreach (var g in client.Guilds)
                    {
                        if (Config.BlacklistedOwners.Contains(g.OwnerId))
                            await g.LeaveAsync().Then(async () => Warn(LogSource.Volte,
                                $"Left guild \"{g.Name}\" owned by blacklisted owner {await client.Rest.GetUserAsync(g.OwnerId)}."));
                        else provider.Get<DatabaseService>().GetData(g); //ensuring all guilds have data available to prevent exceptions later on 
                    }
                });
            };
            
            if (provider.TryGet<GuildService>(out var guild))
            {
                client.JoinedGuild += async g => await guild.OnJoinAsync(new JoinedGuildEventArgs(g));
            }

            client.UserJoined += async user =>
            {
                if (Config.EnabledFeatures.Welcome) await welcome.JoinAsync(new UserJoinedEventArgs(user));
                if (Config.EnabledFeatures.Autorole) await autorole.ApplyRoleAsync(new UserJoinedEventArgs(user));
                if (provider.Get<DatabaseService>().GetData(user.Guild).Configuration.Moderation.CheckAccountAge &&
                    Config.EnabledFeatures.ModLog)
                    await mod.CheckAccountAgeAsync(new UserJoinedEventArgs(user));
            };

            client.UserLeft += async (guild, user) =>
            {
                if (Config.EnabledFeatures.Welcome) await welcome.LeaveAsync(new UserLeftEventArgs(guild, user));
            };
            
            client.ReactionAdded += (message, channel, reaction) => starboard.HandleReactionAddAsync(message, channel, reaction);
            client.ReactionRemoved += (message, channel, reaction) => starboard.HandleReactionRemoveAsync(message, channel, reaction);
            client.ReactionsCleared += (message, channel) => starboard.HandleReactionsClearAsync(message, channel);
        }

        public static Task<IUserMessage> SendToAsync(this EmbedBuilder e, IMessageChannel c) =>
            c.SendMessageAsync(embed: e.Build(), allowedMentions: AllowedMentions.None);

        public static Task<IUserMessage> SendToAsync(this Embed e, IMessageChannel c) =>
            c.SendMessageAsync(embed: e, allowedMentions: AllowedMentions.None);

        public static Task<IUserMessage> ReplyToAsync(this EmbedBuilder e, IUserMessage msg) =>
            msg.ReplyAsync(embed: e.Build(), allowedMentions: AllowedMentions.None);

        public static Task<IUserMessage> ReplyToAsync(this Embed e, IUserMessage msg) =>
            msg.ReplyAsync(embed: e, allowedMentions: AllowedMentions.None);


        // ReSharper disable twice UnusedMethodReturnValue.Global
        public static async Task<IUserMessage> SendToAsync(this EmbedBuilder e, IGuildUser u) =>
            await (await u.CreateDMChannelAsync()).SendMessageAsync(embed: e.Build());

        public static async Task<IUserMessage> SendToAsync(this Embed e, IGuildUser u) =>
            await (await u.CreateDMChannelAsync()).SendMessageAsync(embed: e);

        public static Emoji ToEmoji(this string str) => new Emoji(str);

        public static bool ShouldHandle(this SocketMessage message, out SocketUserMessage userMessage)
        {
            if (message is SocketUserMessage msg && !msg.Author.IsBot)
            {
                userMessage = msg;
                return true;
            }

            userMessage = null;
            return false;
        }

        public static async Task<bool> TryDeleteAsync(this IDeletable deletable, RequestOptions options = null)
        {
            try
            {
                if (deletable is null) return false;
                await deletable.DeleteAsync(options);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Task<bool> TryDeleteAsync(this IDeletable deletable, string reason)
            => deletable.TryDeleteAsync(CreateRequestOptions(opts => opts.AuditLogReason = reason));

        public static string GetEffectiveUsername(this IGuildUser user) =>
            user.Nickname ?? user.Username;

        public static string GetEffectiveAvatarUrl(this IUser user, ImageFormat format = ImageFormat.Auto,
            ushort size = 128)
            => user.GetAvatarUrl(format, size) ?? user.GetDefaultAvatarUrl();

        public static bool HasAttachments(this IMessage message)
            => message.Attachments.Count != 0;

        public static bool HasColor(this IRole role)
            => role.Color.RawValue != 0;

        public static EmbedBuilder WithDescription(this EmbedBuilder e, StringBuilder sb)
            => e.WithDescription(sb.ToString());
    }

    public enum TimestampType : sbyte
    {
        ShortTime = (sbyte)'t',
        LongTime = (sbyte)'T',
        ShortDate = (sbyte)'d',
        LongDate = (sbyte)'D',
        ShortDateTime = (sbyte)'f',
        LongDateTime = (sbyte)'F',
        Relative = (sbyte)'R'
    }
    
}
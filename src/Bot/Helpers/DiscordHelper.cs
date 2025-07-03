using JetBrains.Annotations;
using RyuBot.Interactions;

namespace RyuBot.Helpers;

public static class DiscordHelper
{
    public static ulong DevGuildId = 1294443224030511104;
    
    public static string Zws => "\u200B";

    public static RequestOptions RequestOptions(Action<RequestOptions> initializer) 
        => new RequestOptions().Apply(initializer);


    /// <summary>
    ///     Checks if the current user is the user identified in the bot's config.
    /// </summary>
    /// <param name="user">The current user</param>
    /// <returns>True, if the current user is the bot's owner; false otherwise.</returns>
    public static bool IsBotOwner(this IUser user)
        => Config.Owner == user.Id;

    private static bool IsGuildOwner(this IGuildUser user)
        => user.Guild.OwnerId == user.Id || IsBotOwner(user);

    public static bool HasRole(this SocketGuildUser user, ulong roleId)
        => user.Roles.Select(x => x.Id).Contains(roleId);

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

    public static SocketRole GetHighestRole(this SocketUser user, bool requireColor = true)
    {
        if (user is not SocketGuildUser sgu) return null;
        
        return sgu.Roles
            .Where(x => !requireColor || x.HasColor)
            .MaxBy(static x => x.Position);
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
    
    public static void RegisterVolteEventHandlers(this DiscordSocketClient client, ServiceProvider provider)
    {
        Listen(client);

        client.Ready += async () =>
        {
            var guilds = client.Guilds.Count;

            PrintHeader();
            Info(LogSource.Bot, $"Currently running RyuBot V{Version.InformationVersion}.");
            Info(LogSource.Bot, "Use this URL to invite me to your guilds:");
            Info(LogSource.Bot, client.GetInviteUrl());
            Info(LogSource.Bot, $"Logged in as {client.CurrentUser.Username}#{client.CurrentUser.Discriminator}");
            Info(LogSource.Bot, $"Connected to {"guild".ToQuantity(guilds)}");

            if (Config.TryParseActivity(out var activityInfo))
            {
                if (activityInfo.Streamer is null && activityInfo.Type != ActivityType.CustomStatus)
                {
                    await client.SetGameAsync(activityInfo.Name, null, activityInfo.Type);
                    Info(LogSource.Bot, $"Set {client.CurrentUser.Username}'s game to \"{Config.Game}\".");
                }
                else if (activityInfo.Type != ActivityType.CustomStatus)
                {
                    await client.SetGameAsync(activityInfo.Name, Config.FormattedStreamUrl, activityInfo.Type);
                    Info(LogSource.Bot,
                        $"Set {client.CurrentUser.Username}'s activity to \"{activityInfo.Type}: {activityInfo.Name}\", at Twitch user {Config.Streamer}.");
                }
            }
            
            ExecuteBackgroundAsync(() => provider.Get<RyujinxBotInteractionService>().InitAsync());
        };
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
        await e.SendToAsync(await u.CreateDMChannelAsync());

    public static async Task<IUserMessage> SendToAsync(this Embed e, IGuildUser u) =>
       await e.SendToAsync(await u.CreateDMChannelAsync());

    public static Emoji ToEmoji(this string str) => new(str);

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
        => deletable.TryDeleteAsync(RequestOptions(opts => opts.AuditLogReason = reason));

    public static string GetEffectiveAvatarUrl(this IUser user, ImageFormat format = ImageFormat.Auto,
        ushort size = 128)
        => user.GetAvatarUrl(format, size) ?? user.GetDefaultAvatarUrl();

    extension(BaseSocketClient client)
    {
        [CanBeNull] public SocketUser BotOwner => client.GetUser(Config.Owner);
    }
    
    extension(IGuildUser user)
    {
        public string EffectiveDisplayName => user.Nickname ?? user.DisplayName ?? user.Username;
    }

    extension(IRole role)
    {
        public bool HasColor => role.Color.RawValue != 0;
    }

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
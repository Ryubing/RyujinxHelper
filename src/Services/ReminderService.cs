using System.Text.RegularExpressions;

namespace Volte.Services;

public partial class ReminderService(
    DatabaseService _db,
    DiscordSocketClient _client)
    : VolteService, IDisposable
{
    private static readonly Regex JumpUrl = MessageUrlPattern();
    
    private readonly PeriodicTimer _ticker = new(30.Seconds());

    public readonly CancellationTokenSource TickerTokenSource = new();

    /// <summary>
    ///     If its value is already set; this method returns immediately.
    /// </summary>
    public void Initialize() =>
        Executor.ExecuteBackgroundAsync(async () =>
        {
            while (await _ticker.WaitForNextTickAsync(TickerTokenSource.Token))
            {
                Debug(LogSource.Service, "Checking all reminders.");
                foreach (var (reminder, index) in _db.GetAllReminders().WithIndex()) 
                {
                    Debug(LogSource.Service,
                        $"Reminder '{reminder.ReminderText}', set for {reminder.TargetTime} at index {index}");
                    if (reminder.TargetTime.Ticks <= DateTime.Now.Ticks) 
                        await SendAsync(reminder);
                }
            }
        });

    private async Task SendAsync(Reminder reminder)
    {
        var guild = _client.GetGuild(reminder.GuildId);
        var channel = guild?.GetTextChannel(reminder.ChannelId);
        if (channel is null)
        {
            if (_db.TryDeleteReminder(reminder))
                Debug(LogSource.Service,
                    "Reminder deleted from the database as Volte no longer has access to the channel it was created in.");
            Debug(LogSource.Service,
                "Reminder's target channel was no longer accessible in the guild; aborting.");
            return;
        }

        var author = guild.GetUser(reminder.CreatorId);
        if (author is null)
        {
            if (_db.TryDeleteReminder(reminder))
                Debug(LogSource.Service,
                    "Reminder deleted from the database as its creator is no longer in the guild it was made.");
            Debug(LogSource.Service, "Reminder's creator was no longer present in the guild; aborting.");
            return;
        }

        var timestamp = (await channel.GetMessageAsync(reminder.MessageId).AsOptional())
            .Convert(msg => Format.Url(reminder.CreationTime.ToDiscordTimestamp(TimestampType.Relative), msg.GetJumpUrl()))
            .OrElseGet(() => reminder.CreationTime.ToDiscordTimestamp(TimestampType.Relative));

        await channel.SendMessageAsync(author.Mention, embed: new EmbedBuilder()
            .WithTitle("Reminder")
            .WithRelevantColor(author)
            .WithDescription(IsMessageUrl(reminder)
                ? $"You asked me {timestamp} to remind you about {Format.Url("this message", reminder.ReminderText)}."
                : $"You asked me {timestamp} to remind you about:\n{new string('-', 20)} {reminder.ReminderText}")
            .Build());
        _db.TryDeleteReminder(reminder);
    }

    private static bool IsMessageUrl(Reminder reminder) => JumpUrl.IsMatch(reminder.ReminderText, out var match) &&
                                                           (match.Groups["GuildId"].Value is "@me" ||
                                                            match.Groups["GuildId"].Value.TryParse<ulong>(out _));
        
    [GeneratedRegex(@"https?://(?:(?:ptb|canary)\.)?discord(app)?\.com/channels/(?<GuildId>.+)/(?<ChannelId>\d+)/(?<MessageId>\d+)/?", RegexOptions.Compiled)]
    private static partial Regex MessageUrlPattern();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        TickerTokenSource?.Cancel();
            
        _ticker?.Dispose();
        TickerTokenSource?.Dispose();
    }
}
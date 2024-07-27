namespace Volte.Services;

public sealed class MessageService : VolteService
{
    private readonly CommandService _commandService;
    private readonly QuoteService _quoteService;
        
    public MessageService(DiscordSocketClient client,
        IServiceProvider provider,
        CommandService commandService,
        QuoteService quoteService)
    {
        _commandService = commandService;
        _quoteService = quoteService;
        CalledCommandsInfo.StartPersistence(provider, saveEvery: 2.Minutes());
        
        client.MessageReceived += async socketMessage =>
        {
            if (socketMessage.ShouldHandle(out var msg))
            {
                if (msg.Channel is IDMChannel dm)
                    await dm.SendMessageAsync("Currently, I do not support commands via DM.");
                else
                    await HandleMessageAsync(new MessageReceivedEventArgs(socketMessage, provider));
            }
        };
    }
    
    public ulong AllTimeCommandCalls => CalledCommandsInfo.Sum + 
                                        UnsavedSuccessfulCommandCalls + 
                                        UnsavedFailedCommandCalls;
    public ulong AllTimeSuccessfulCommandCalls => CalledCommandsInfo.Successes + UnsavedSuccessfulCommandCalls;
    public ulong AllTimeFailedCommandCalls => CalledCommandsInfo.Failures + UnsavedFailedCommandCalls;
    
    public ulong UnsavedSuccessfulCommandCalls { get; private set; }
    public ulong UnsavedFailedCommandCalls { get; private set; }

    public void ResetCalledCommands()
    {
        UnsavedSuccessfulCommandCalls = 0;
        UnsavedFailedCommandCalls = 0;
    }

    public async Task HandleMessageAsync(MessageReceivedEventArgs args)
    {
        List<string> prefixes = [
            args.Data.Configuration.CommandPrefix, 
            $"<@{args.Context.Client.CurrentUser.Id}> ",
            $"<@!{args.Context.Client.CurrentUser.Id}> "
        ];

        if (CommandUtilities.HasAnyPrefix(args.Message.Content, prefixes, StringComparison.OrdinalIgnoreCase, out _,
                out var cmd))
        {
            var sw = Stopwatch.StartNew();
            var result = await _commandService.ExecuteAsync(cmd, args.Context);
            sw.Stop();
            if (result is not CommandNotFoundResult)
                await OnCommandAsync(new CommandCalledEventArgs(result, args.Context, sw));
        }
        else
        {
            if (args.Message.Content.EqualsAnyIgnoreCase($"<@{args.Context.Client.CurrentUser.Id}>",
                    $"<@!{args.Context.Client.CurrentUser.Id}>"))
            {
                await args.Context.CreateEmbed(
                        $"The prefix for this guild is **{args.Data.Configuration.CommandPrefix}**; " +
                        $"alternatively you can just mention me as a prefix, i.e. `@{args.Context.Guild.CurrentUser} help`.")
                    .ReplyToAsync(args.Message);
            }
            else if (!await _quoteService.CheckMessageAsync(args))
                if (CommandUtilities.HasPrefix(args.Message.Content, '%', out var tagName))
                {
                    await args.Context.GuildData.Extras.Tags
                        .FindFirst(t => t.Name.EqualsIgnoreCase(tagName))
                        .IfPresent(async tag => 
                        { 
                            if (args.Context.GuildData.Configuration.EmbedTagsAndShowAuthor) 
                                await tag.AsEmbed(args.Context).SendToAsync(args.Message.Channel);
                            else 
                                await args.Message.Channel.SendMessageAsync(tag.FormatContent(args.Context)); 
                        });
                }
                
        }
    }

    private async Task OnCommandAsync(CommandCalledEventArgs args)
    {
        Gommon.Optional<ResultCompletionData> data;
        switch (args.Result)
        {
            case ActionResult actionRes:
            {
                data = await actionRes.ExecuteResultAsync(args.Context);
                Debug(LogSource.Service,
                    $"Executed {args.Context.Command.Name}'s resulting {actionRes.GetType().AsPrettyString()}.");

                if (actionRes is BadRequestResult badreq)
                {
                    UnsavedFailedCommandCalls += 1;
                    OnBadRequest(new CommandBadRequestEventArgs(badreq, data, args));
                    return;
                }

                break;
            }

            case FailedResult failedRes:
            {
                UnsavedFailedCommandCalls += 1;
                await OnCommandFailureAsync(new CommandFailedEventArgs(failedRes, args));
                return;
            }

            default:
            {
                Error(LogSource.Volte, "---------- IMPORTANT ----------");
                Error(LogSource.Service,
                    $"The command {args.Context.Command.Name} didn't return some form of {typeof(ActionResult)}. " +
                    "This is developer error. " +
                    "Please report this to my developers: https://github.com/Polyhaze/Volte. Thank you!");
                Error(LogSource.Volte, "---------- IMPORTANT ----------");
                return;
            }
        }

        UnsavedSuccessfulCommandCalls += 1;
        if (!Config.LogAllCommands) return;

        var sb = new StringBuilder()
            .AppendLine(CommandFrom(args))
            .AppendLine(CommandIssued(args))
            .AppendLine(FullMessage(args))
            .AppendLine(InGuild(args))
            .AppendLine(InChannel(args))
            .AppendLine(TimeIssued(args))
            .AppendLine(args.ExecutedLogMessage())
            .AppendLine(After(args));
        
        if (data)
            sb.AppendLine(ResultMessage(data));

        sb.Append(Separator);
        Info(LogSource.Volte, sb.ToString());
    }

    private static async Task OnCommandFailureAsync(CommandFailedEventArgs args)
    {
        var reason = args.Result switch
        {
            CommandNotFoundResult => "Unknown command.",
            ChecksFailedResult cfr => checksFailed(cfr),
            ParameterChecksFailedResult pcfr => paramChecksFailed(pcfr),
            ArgumentParseFailedResult apfr => $"Parsing for arguments failed for {Format.Bold(apfr.Command.Name)}.",
            TypeParseFailedResult tpfr => tpfr.FailureReason,
            OverloadsFailedResult => "A suitable overload could not be found for the given parameter type/order.",
            CommandExecutionFailedResult cefr => executionFailed(cefr),
            _ => unknown(args.Result)
        };

        if (!reason.IsNullOrEmpty())
        {
            await args.Context.CreateEmbedBuilder()
                .AddField("Error in Command", args.Context.Command.Name)
                .AddField("Error Reason", reason)
                .AddField("Usage", TextCommandHelper.FormatUsage(args.Context, args.Context.Command))
                .SendToAsync(args.Context.Channel);

            if (!Config.LogAllCommands) return;

            Error(LogSource.Module, new StringBuilder()
                .AppendLine(CommandFrom(args))
                .AppendLine(CommandIssued(args))
                .AppendLine(FullMessage(args))
                .AppendLine(InGuild(args))
                .AppendLine(InChannel(args))
                .AppendLine(TimeIssued(args))
                .AppendLine(args.ExecutedLogMessage(reason))
                .AppendLine(After(args))
                .Append(Separator).ToString());
        }

        return;


        static string checksFailed(ChecksFailedResult result) 
            => String(sb => sb
                .Append("One or more checks failed for command ")
                .Append(Format.Bold(result.Command.Name))
                .AppendLine(":")
                .Append(Format.Code(result.FailedChecks.Select(x => 
                    $"{Format.Code(x.Check.GetType().AsPrettyString().Replace("Attribute", ""))}: {x.Result.FailureReason}"
                ).JoinToString('\n'), "css"))
            );


        static string paramChecksFailed(ParameterChecksFailedResult result) 
            => String(sb => sb
                .Append("One or more checks failed on parameter ")
                .Append(Format.Bold(result.Parameter.Name))
                .AppendLine(":")
                .Append(Format.Code(result.FailedChecks.Select(x => 
                    $"{Format.Code(x.Check.GetType().AsPrettyString().Replace("Attribute", ""))}: {x.Result.FailureReason}"
                ).JoinToString('\n'), "css"))
            );
        
        static string unknown(FailedResult result)
        {
            Verbose(LogSource.Service,
                $"A command returned an unknown error. Please screenshot this message and show it to my developers: {result.GetType().AsPrettyString()}->{result.FailureReason}");
            return "Unknown error.";
        }

        static string executionFailed(CommandExecutionFailedResult result)
        {
            Error(result.Exception);
            return $"Execution of this command failed. Exception: {result.Exception.GetType().AsPrettyString()}";
        }
    }

    private static void OnBadRequest(CommandBadRequestEventArgs args)
    {
        var sb = new StringBuilder()
            .AppendLine(CommandFrom(args))
            .AppendLine(CommandIssued(args))
            .AppendLine(FullMessage(args))
            .AppendLine(InGuild(args))
            .AppendLine(InChannel(args))
            .AppendLine(TimeIssued(args))
            .AppendLine(args.ExecutedLogMessage())
            .AppendLine(After(args))
            .AppendLine(ResultMessage(args.ResultCompletionData));

        sb.Append(Separator);
        Error(LogSource.Module, sb.ToString());
    }

    private const int SpaceCount = 20;
    private const int HyphenCount = 49;

    public static readonly string Separator = string.Intern(
        String(sb => sb
            .Append(string.Intern(new string(' ', SpaceCount)))
            .Append(new string('-', HyphenCount))
        ));

    private static string CommandFrom(CommandEventArgs args) => 
        $"|  -Command from user: {args.Context.User} ({args.Context.User.Id})";

    private static string CommandIssued(CommandEventArgs args) 
        => new StringBuilder(string.Intern(new string(' ', SpaceCount)))
            .Append($"|     -Command Issued: {args.Context.Command.Name}").ToString();

    private static string FullMessage(CommandEventArgs args) 
        => new StringBuilder(string.Intern(new string(' ', SpaceCount)))
            .Append($"|       -Full Message: {args.Context.Message.Content}").ToString();

    private static string InGuild(CommandEventArgs args) 
        => new StringBuilder(string.Intern(new string(' ', SpaceCount)))
            .Append($"|           -In Guild: {args.Context.Guild.Name} ({args.Context.Guild.Id})").ToString();

    private static string InChannel(CommandEventArgs args) 
        => new StringBuilder(string.Intern(new string(' ', SpaceCount)))
            .Append($"|         -In Channel: #{args.Context.Channel.Name} ({args.Context.Channel.Id})").ToString();

    private static string TimeIssued(CommandEventArgs args) 
        => new StringBuilder(string.Intern(new string(' ', SpaceCount)))
            .Append($"|        -Time Issued: {args.Context.Now.FormatFullTime()}, {args.Context.Now.FormatDate()}").ToString();

    private static string After(CommandEventArgs args) 
        => new StringBuilder(string.Intern(new string(' ', SpaceCount)))
            .Append($"|              -After: {args.Stopwatch.Elapsed.Humanize()}").ToString();

    private static string ResultMessage(ResultCompletionData data) 
        => new StringBuilder(string.Intern(new string(' ', SpaceCount)))
            .Append($"|     -Result Message: {data.Message?.Id}").ToString();
}
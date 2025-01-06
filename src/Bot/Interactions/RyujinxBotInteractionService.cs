using Discord.Interactions;
using RyuBot.Entities;
using RyuBot.Helpers;
using RyuBot.Interactions.Results;
using RyuBot.Services;
using IResult = Discord.Interactions.IResult;

namespace RyuBot.Interactions;

public class RyujinxBotInteractionService : BotService
{
    private static bool _commandsRegistered;
    
    private readonly IServiceProvider _provider;
    private readonly InteractionService _backing;

    public RyujinxBotInteractionService(IServiceProvider provider, DiscordSocketClient client)
    {
        _provider = provider;
        _backing = new(client.Rest, new()
        {
            LogLevel = Config.DebugEnabled || Version.IsDevelopment
                ? LogSeverity.Debug
                : LogSeverity.Verbose,
            InteractionCustomIdDelimiters = [MessageComponentId.Separator]
        });

        {
            client.SlashCommandExecuted += async interaction =>
            {
                var ctx = new SocketInteractionContext<SocketSlashCommand>(client, interaction);
                if (!IsInAllowedGuild(interaction)) return;
                await _backing.ExecuteCommandAsync(ctx, provider);
            };

            client.MessageCommandExecuted += async interaction =>
            {
                var ctx = new SocketInteractionContext<SocketMessageCommand>(client, interaction);
                if (!IsInAllowedGuild(interaction)) return;
                await _backing.ExecuteCommandAsync(ctx, provider);
            };

            client.UserCommandExecuted += async interaction =>
            {
                var ctx = new SocketInteractionContext<SocketUserCommand>(client, interaction);
                if (!IsInAllowedGuild(interaction)) return;
                await _backing.ExecuteCommandAsync(ctx, provider);
            };
            
            client.AutocompleteExecuted += async interaction =>
            {
                var ctx = new SocketInteractionContext<SocketAutocompleteInteraction>(client, interaction);
                if (!IsInAllowedGuild(interaction)) return;
                await _backing.ExecuteCommandAsync(ctx, _provider);
            };
        }

        _backing.Log += logMessage =>
        {
            Log(new VolteLogEventArgs(logMessage));
            return Task.CompletedTask;
        };

        _backing.SlashCommandExecuted += (info, context, result) => 
            OnSlashCommandExecuted(info, context.Cast<SocketInteractionContext<SocketSlashCommand>>(), result);
        _backing.ContextCommandExecuted += async (info, context, result) =>
        {
            if (context.Interaction is SocketMessageCommand)
                await OnContextCommandExecuted(info, context.Cast<SocketInteractionContext<SocketMessageCommand>>(),
                    result);
            else
                await OnContextCommandExecuted(info, context.Cast<SocketInteractionContext<SocketUserCommand>>(),
                    result);
        };
    }
    
    private static bool IsInAllowedGuild(SocketInteraction interaction) 
        => Config.WhitelistGuildRepos.ContainsKey(interaction.GuildId ?? ulong.MaxValue);

    public async Task<int> ClearAllCommandsInGuildAsync(ulong guildId)
    {
        var modules = _backing.Modules.ToArray();
        var commandsRemoved = 0;
        commandsRemoved += modules.Sum(x => x.ComponentCommands.Count);
        commandsRemoved += modules.Sum(x => x.ContextCommands.Count);
        commandsRemoved += modules.Sum(x => x.AutocompleteCommands.Count);
        commandsRemoved += modules.Sum(x => x.ModalCommands.Count);
        commandsRemoved += modules.Sum(x => x.SlashCommands.Count);
        
        await _backing.RemoveModulesFromGuildAsync(guildId, _backing.Modules.ToArray());

        return commandsRemoved;
    }
    
    public async Task InitAsync()
    {
        if (!_commandsRegistered)
        {
            await _backing.AddModulesAsync(Assembly.GetExecutingAssembly(), _provider);
#if DEBUG
            await _backing.RegisterCommandsToGuildAsync(DiscordHelper.DevGuildId);
#else
            await Config.WhitelistGuilds
                .ForEachAsync(async id => await _backing.RegisterCommandsToGuildAsync(id));
#endif
            
            _commandsRegistered = true;
        }
    }

    #region Event Handlers

    private static Task OnSlashCommandExecuted(
        SlashCommandInfo commandInfo,
        SocketInteractionContext<SocketSlashCommand> context, 
        IResult result
    ) => OnCommandExecuted<SocketSlashCommand, SlashCommandInfo, SlashCommandParameterInfo>(commandInfo, context, result);

    private static Task OnContextCommandExecuted<TInteraction>(
        ContextCommandInfo commandInfo,
        SocketInteractionContext<TInteraction> context,
        IResult result
    ) where TInteraction : SocketInteraction
        => OnCommandExecuted<TInteraction, ContextCommandInfo, CommandParameterInfo>(commandInfo, context, result);

    private static async Task OnCommandExecuted<TInteraction, TCommandInfo, TParameterInfo>(TCommandInfo command,
        SocketInteractionContext<TInteraction> context, IResult result)
        where TInteraction : SocketInteraction 
        where TParameterInfo : CommandParameterInfo
        where TCommandInfo : CommandInfo<TParameterInfo>
    {
        if (result is null) return;

        switch (result)
        {
            case BotResultBase botResult:
                await botResult.ExecuteAsync();
                break;
            case PreconditionResult unmetPreconditionResult:
                await context.CreateReplyBuilder(true)
                    .WithEmbed(e =>
                        e.WithTitle(unmetPreconditionResult.ErrorReason)
                            .WithColor(Color.DarkRed)
                            .WithCurrentTimestamp()
                    ).ExecuteAsync();
                break;
            // for when dnet fixes their fucking shit
            //case ExecuteResult errorResult:
            //    Error(LogSource.Service, $"Error occurred executing command {command.Name}", errorResult.Exception);
            //    break;
        }
    }

    #endregion Event Handlers
}
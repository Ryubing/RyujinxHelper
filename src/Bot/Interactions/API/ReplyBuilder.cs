using Discord.Interactions;
// ReSharper disable MemberCanBePrivate.Global

namespace Volte.Interactions;

#nullable enable
public class ReplyBuilder<TInteraction> where TInteraction : SocketInteraction
{
    public SocketInteractionContext<TInteraction> Context { get; }

    public string? Content { get; private set; }
    public HashSet<Embed> Embeds { get; } = [];
    public bool IsTts { get; private set; }
    public bool IsEphemeral { get; private set; }
    public AllowedMentions AllowedMentions { get; private set; } = AllowedMentions.None;
    public Task UpdateOrNoopTask => _updateTask ?? Task.CompletedTask;
    private Task? _updateTask;
    public HashSet<ActionRowBuilder> ActionRows { get; } = [];

    public ReplyBuilder(SocketInteractionContext<TInteraction> ctx) => Context = ctx;

    public ReplyBuilder<TInteraction> WithContent(string content)
    {
        Content = content;
        return this;
    }

    public ReplyBuilder<TInteraction> WithEmbed(Action<EmbedBuilder> modifier)
    {
        Embeds.Add(Context.CreateEmbedBuilder().Apply(modifier).Build());
        return this;
    }

    public ReplyBuilder<TInteraction> WithEmbeds(IEnumerable<EmbedBuilder> embedBuilders)
    {
        embedBuilders.ForEach(x => Embeds.Add(x.Build()));
        return this;
    }

    public ReplyBuilder<TInteraction> WithEmbeds(IEnumerable<Embed> embeds)
    {
        embeds.ForEach(x => Embeds.Add(x));
        return this;
    }

    public ReplyBuilder<TInteraction> WithEmbeds(params EmbedBuilder[] embedBuilders) =>
        WithEmbeds(embedBuilders.Select(x => x.Build()));

    public ReplyBuilder<TInteraction> WithEmbeds(params Embed[] embeds) => WithEmbeds(embeds.ToList());

    public ReplyBuilder<TInteraction> WithEmbedFrom(StringBuilder content)
        => WithEmbedFrom(content.ToString());

    public ReplyBuilder<TInteraction> WithEmbedFrom(string content)
    {
        Embeds.Add(Context.CreateEmbedBuilder(content).Build());
        return this;
    }

    public ReplyBuilder<TInteraction> WithTts(bool tts)
    {
        IsTts = tts;
        return this;
    }

    public ReplyBuilder<TInteraction> WithEphemeral(bool ephemeral = true)
    {
        IsEphemeral = ephemeral;
        return this;
    }

    public ReplyBuilder<TInteraction> WithAllowedMentions(AllowedMentions allowedMentions)
    {
        AllowedMentions = allowedMentions;
        return this;
    }

    public ReplyBuilder<TInteraction> WithComponentMessageUpdate(Action<MessageProperties> modifier,
        RequestOptions? options = null)
    {
        if (Context.Interaction is SocketMessageComponent smc)
            _updateTask = smc.UpdateAsync(modifier, options);

        return this;
    }

    public ReplyBuilder<TInteraction> WithComponent(ComponentBuilder builder) 
        => WithActionRows(builder.ActionRows);

    public ReplyBuilder<TInteraction> WithActionRows(params ActionRowBuilder[] actionRows)
    {
        actionRows.ForEach(row => ActionRows.Add(row));
        return this;
    }

    public ReplyBuilder<TInteraction> WithActionRows(IEnumerable<ActionRowBuilder> actionRows)
        => WithActionRows(actionRows.ToArray());

    public ReplyBuilder<TInteraction> WithButtons(IEnumerable<ButtonBuilder> buttons)
        => WithButtons(buttons.ToArray());

    public ReplyBuilder<TInteraction> WithButtons(params ButtonBuilder[] buttons)
        => WithActionRows(buttons.Select(x => x.Build()).AsActionRow());


    public ReplyBuilder<TInteraction> WithSelectMenu(SelectMenuBuilder menu)
    {
        ActionRows.Add(new ActionRowBuilder().AddComponent(menu.Build()));
        return this;
    }

    public Task RespondAsync(RequestOptions? options = null)
        => Context.Interaction.RespondAsync(Content, Embeds.ToArray(), IsTts, IsEphemeral,
                AllowedMentions, new ComponentBuilder().AddActionRows(ActionRows).Build(), options: options)
            .Then(() => UpdateOrNoopTask);


    public Task<RestFollowupMessage> FollowupAsync(RequestOptions? options = null)
        => Context.Interaction.FollowupAsync(Content, Embeds.ToArray(), IsTts, IsEphemeral,
                AllowedMentions, new ComponentBuilder().AddActionRows(ActionRows).Build(), options: options)
            .ThenApply(_ => UpdateOrNoopTask);
}
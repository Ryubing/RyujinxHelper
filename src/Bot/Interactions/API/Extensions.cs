﻿using Discord.Interactions;

namespace RyuBot.Interactions;

public static class Extensions
{
    public static SelectMenuBuilder AddOptions(this SelectMenuBuilder menu,
        IEnumerable<SelectMenuOptionBuilder> options)
    {
        options.ForEach(opt => menu.AddOption(opt));
        return menu;
    }

    public static ActionRowBuilder AddComponentIf(this ActionRowBuilder builder, bool condition,
        IMessageComponent component)
    {
        if (condition)
        {
            if (builder.Components.Count >= 5)
                throw new InvalidOperationException("Cannot have more than 5 components in a single Action Row.");

            builder.Components.Add(component);
        }

        return builder;
    }

    public static ActionRowBuilder AsActionRow(this IEnumerable<IMessageComponent> components)
        => new ActionRowBuilder().AddComponents(components);

    public static ActionRowBuilder AddComponents(this ActionRowBuilder builder,
        IEnumerable<IMessageComponent> components)
        => builder.Apply(x => components.ForEach(c => x.AddComponent(c)));

    public static ComponentBuilder AddActionRow(this ComponentBuilder builder,
        Action<ActionRowBuilder> initializer)
        => builder.AddActionRows(new ActionRowBuilder().Apply(initializer));

    public static ComponentBuilder AddActionRows(this ComponentBuilder builder,
        IEnumerable<ActionRowBuilder> actionRows) => builder.AddActionRows(actionRows.ToArray());

    public static ComponentBuilder AddActionRows(this ComponentBuilder builder,
        params ActionRowBuilder[] actionRows)
    {
        builder.ActionRows ??= [];
        builder.ActionRows.AddRange(actionRows);
        return builder;
    }

    public static EmbedBuilder CreateEmbedBuilder(this SocketUserMessage userMessage, string content = null)
        => new EmbedBuilder().WithDescription(content ?? string.Empty)
            .WithColor(userMessage.Author.Cast<SocketGuildUser>()?.GetHighestRole()?.Color ?? Config.SuccessColor);

    public static MessageComponentId GetId(this IComponentInteraction interaction)
        => interaction.Data.CustomId;
    
    public static MessageComponentId GetId(this SocketInteractionContext<SocketMessageComponent> ctx)
        => ctx.Interaction.Data.CustomId;
    
    public static ReplyBuilder<TInteraction> CreateReplyBuilder<TInteraction>(
        this SocketInteractionContext<TInteraction> context,
        bool ephemeral = false,
        bool deferred = false
    ) where TInteraction : SocketInteraction
        => new ReplyBuilder<TInteraction>(context)
            .WithEphemeral(ephemeral)
            .WithDeferral(deferred);
}
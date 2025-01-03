using RyuBot.Entities;

namespace RyuBot.Helpers;

public static class EmbedHelpers
{
    public static EmbedBuilder WithSuccessColor(this EmbedBuilder e) => e.WithColor(Config.SuccessColor);

    public static EmbedBuilder WithErrorColor(this EmbedBuilder e) => e.WithColor(Config.ErrorColor);

    public static EmbedBuilder WithRelevantColor(this EmbedBuilder e, SocketGuildUser user) =>
        e.WithColor(user.GetHighestRole()?.Color ?? new Color(Config.SuccessColor));
    
    public static EmbedBuilder WithDescription(this EmbedBuilder e, Action<StringBuilder> description) => 
        e.WithDescription(String(description));

    public static EmbedBuilder AppendDescription(this EmbedBuilder e, string toAppend) =>
        e.WithDescription((e.Description ?? string.Empty) + toAppend);
    
    public static EmbedBuilder AppendDescriptionLine(this EmbedBuilder e, string toAppend = "") =>
        e.AppendDescription($"{toAppend}\n");

    public static EmbedBuilder AddField(this EmbedBuilder e, object name, Action<StringBuilder> description,
        bool inline = false) => 
        e.AddField(name.ToString(), String(description), inline);
    
    public static Embed Embed(Action<EmbedBuilder> initializer) => new EmbedBuilder().Apply(initializer).Build();
}
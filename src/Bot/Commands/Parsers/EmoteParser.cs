namespace RyuBot.Commands.Text;

[InjectTypeParser]
public sealed class EmoteParser : VolteTypeParser<Emote>
{
    public override ValueTask<TypeParserResult<Emote>> ParseAsync(string value, RyujinxBotContext _)
        => Emote.TryParse(value, out var emote)
            ? Success(emote)
            : Failure("Emote not found.");
}
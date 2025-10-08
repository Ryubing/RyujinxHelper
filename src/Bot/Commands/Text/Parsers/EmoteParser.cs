using Qmmands;

namespace RyuBot.Commands.Text;

[InjectTypeParser]
public sealed class EmoteParser : ParameterUnawareTypeParser<Emote>
{
    public override ValueTask<TypeParserResult<Emote>> ParseAsync(string value, BotContext _)
        => Emote.TryParse(value, out var emote)
            ? Success(emote)
            : Failure("Emote not found.");
}
using Qmmands;

namespace RyuBot.Commands.Text;

[InjectTypeParser]
public sealed class UnixParser : ParameterUnawareTypeParser<Dictionary<string, string>>
{
    public override ValueTask<TypeParserResult<Dictionary<string, string>>> ParseAsync(string value, BotContext _) 
        => UnixHelper.TryParseNamedArguments(value, out var result)
            ? Success(result.Parsed)
            : Failure(result.Error.Message);
}
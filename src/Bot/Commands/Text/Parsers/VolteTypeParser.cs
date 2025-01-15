using Qmmands;

namespace RyuBot.Commands.Text;

public abstract class VolteTypeParser<T> : TypeParser<T>
{
    public abstract ValueTask<TypeParserResult<T>> ParseAsync(string value, BotContext ctx);

    public override ValueTask<TypeParserResult<T>> ParseAsync(Parameter _, string value,
        CommandContext context) => ParseAsync(value, context.Cast<BotContext>());
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class InjectTypeParserAttribute(bool overridePrimitive = false) : Attribute
{
    public bool OverridePrimitive => overridePrimitive;
}
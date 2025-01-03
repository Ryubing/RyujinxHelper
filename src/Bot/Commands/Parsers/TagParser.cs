using RyuBot.Entities;

namespace RyuBot.Commands.Text;

[InjectTypeParser]
public sealed class TagParser : VolteTypeParser<Tag>
{
    public override ValueTask<TypeParserResult<Tag>> ParseAsync(string value, RyujinxBotContext ctx)
    {
        if (ctx.GuildData.Extras.Tags.TryGetFirst(x => x.Name.EqualsIgnoreCase(value), out var tag))
            return Success(tag);

        return Failure($"The tag **{value}** doesn't exist in this guild. " +
                       $"Try using the `{ctx.FormatUsageFor("Tags List")}` command to see all tags in this guild.");
    }
}
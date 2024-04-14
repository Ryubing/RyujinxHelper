using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using Volte.Core.Entities;

namespace Volte.Commands
{
    [InjectTypeParser]
    public sealed class EmoteParser : VolteTypeParser<Emote>
    {
        public override ValueTask<TypeParserResult<Emote>> ParseAsync(string value, VolteContext _)
            => Emote.TryParse(value, out var emote)
                ? Success(emote)
                : Failure("Emote not found.");
    }
}
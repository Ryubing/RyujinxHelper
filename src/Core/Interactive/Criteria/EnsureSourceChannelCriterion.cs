using System.Threading.Tasks;
using Discord;
using Volte.Commands.Text;

namespace Volte.Interactive
{
    public class EnsureSourceChannelCriterion : ICriterion<IMessage>
    {
        public ValueTask<bool> JudgeAsync(VolteContext sourceContext, IMessage parameter) 
            => new ValueTask<bool>(sourceContext.Channel.Id == parameter.Channel.Id);

    }
}

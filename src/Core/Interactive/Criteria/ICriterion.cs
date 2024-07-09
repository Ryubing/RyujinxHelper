using System.Threading.Tasks;
using Discord.Commands;
using Volte.Commands.Text;

namespace Volte.Interactive
{
    public interface ICriterion<in T>
    {
        ValueTask<bool> JudgeAsync(VolteContext sourceContext, T parameter);
    }
}

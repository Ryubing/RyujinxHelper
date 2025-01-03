using RyuBot.Commands.Text;

namespace RyuBot.Interactive;

public interface ICriterion<in T>
{
    ValueTask<bool> JudgeAsync(RyujinxBotContext sourceContext, T parameter);
}
using RyuBot.Commands.Text;

namespace RyuBot.Interactive;

public class EmptyCriterion<T> : ICriterion<T>
{
    public ValueTask<bool> JudgeAsync(RyujinxBotContext sourceData, T parameter)
        => new(true);
}
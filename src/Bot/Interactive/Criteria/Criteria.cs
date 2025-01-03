using RyuBot.Commands.Text;

namespace RyuBot.Interactive;

public class Criteria<T> : ICriterion<T>
{
    public delegate ValueTask<bool> LocalCriteria(RyujinxBotContext ctx, T value);
        
    private readonly HashSet<ICriterion<T>> _critiera = [];
    private readonly HashSet<LocalCriteria> _localCriteria = [];

    public Criteria<T> AddCriterion(ICriterion<T> criterion)
    {
        _critiera.Add(criterion);
        return this;
    }

    public Criteria<T> AddCriterion(LocalCriteria criteria)
    {
        _localCriteria.Add(criteria);
        return this;
    }

    public async ValueTask<bool> JudgeAsync(RyujinxBotContext sourceContext, T parameter)
    {
        foreach (var criterion in _critiera)
        {
            var result = await criterion.JudgeAsync(sourceContext, parameter);
            if (!result) return false;
        }

        foreach (var criteria in _localCriteria)
        {
            var result = await criteria(sourceContext, parameter);
            if (!result) return false;
        }
        return true;
    }
}
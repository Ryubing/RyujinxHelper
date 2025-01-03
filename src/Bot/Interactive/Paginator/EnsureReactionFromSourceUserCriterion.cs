using RyuBot.Commands.Text;

namespace RyuBot.Interactive;

internal class EnsureReactionFromSourceUserCriterion : ICriterion<SocketReaction>
{
    public ValueTask<bool> JudgeAsync(VolteContext sourceContext, SocketReaction parameter) 
        => new(parameter.UserId == sourceContext.User.Id);
}
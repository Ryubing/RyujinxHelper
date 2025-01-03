using RyuBot.Commands.Text;

namespace RyuBot.Interactive;

public class EnsureSourceChannelCriterion : ICriterion<IMessage>
{
    public ValueTask<bool> JudgeAsync(RyujinxBotContext sourceContext, IMessage parameter) 
        => new ValueTask<bool>(sourceContext.Channel.Id == parameter.Channel.Id);

}
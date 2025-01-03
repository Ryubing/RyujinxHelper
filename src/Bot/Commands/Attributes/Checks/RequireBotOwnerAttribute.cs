namespace RyuBot.Commands.Text;

public sealed class RequireBotOwnerAttribute : CheckAttribute
{
    public override ValueTask<CheckResult> CheckAsync(CommandContext context)
    {
        var ctx = context.Cast<RyujinxBotContext>();
        if (ctx.User.IsBotOwner()) return CheckResult.Successful;
            
        return CheckResult.Failed("Insufficient permission.");
    }
}
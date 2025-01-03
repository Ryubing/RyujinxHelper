namespace RyuBot.Commands.Text;

public sealed class RequireGuildModeratorAttribute : CheckAttribute
{
    public override ValueTask<CheckResult> CheckAsync(CommandContext context)
    {
        var ctx = context.Cast<RyujinxBotContext>();
        if (ctx.IsModerator(ctx.User)) return CheckResult.Successful;
            
        return CheckResult.Failed("Insufficient permission.");
    }
}
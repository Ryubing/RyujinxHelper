namespace RyuBot.Commands.Text;

public abstract class ActionResult : CommandResult
{
    public override bool IsSuccessful => true;

    public abstract ValueTask<Gommon.Optional<ResultCompletionData>> ExecuteResultAsync(RyujinxBotContext ctx);

    public static implicit operator Task<ActionResult>(ActionResult res) 
        => Task.FromResult(res);
}
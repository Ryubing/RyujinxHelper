using RyuBot.Entities;
using RyuBot.Interactive;
using RyuBot.Services;

namespace RyuBot.Commands.Text;

public abstract class VolteModule : ModuleBase<VolteContext>
{
    public DatabaseService Db { get; set; }
    public ModerationService ModerationService { get; set; }
    public CommandService CommandService { get; set; }
        
    protected ActionResult Ok(
        string text, 
        MessageCallback afterCompletion = null,
        bool shouldEmbed = true) 
        => new OkResult(text, shouldEmbed, null, afterCompletion);

    protected ActionResult Ok(
        AsyncFunction logic, 
        bool awaitLogic = true) 
        => new OkResult(logic, awaitLogic);

    protected ActionResult Ok(Action<StringBuilder> textBuilder, MessageCallback messageCallback = null,
        bool shouldEmbed = true)
        => Ok(String(textBuilder), messageCallback, shouldEmbed);

    protected ActionResult Ok(StringBuilder text, MessageCallback messageCallback = null, bool shouldEmbed = true)
        => Ok(text.ToString(), messageCallback, shouldEmbed);
    protected ActionResult Ok(PaginatedMessage.Builder pager) => new OkResult(pager);
    protected ActionResult Ok(IEnumerable<EmbedBuilder> embeds) => new OkResult(embeds);

    protected ActionResult Ok(PollInfo pollInfo) => new OkResult(pollInfo);

    protected ActionResult Ok(
        EmbedBuilder embed, 
        MessageCallback afterCompletion = null) 
        => new OkResult(null, true, embed, afterCompletion);

    protected ActionResult Ok(string text) 
        => new OkResult(text);

    protected ActionResult Ok(EmbedBuilder embed) 
        => new OkResult(null, true, embed);

    protected ActionResult BadRequest(string reason) 
        => new BadRequestResult(reason);

    protected ActionResult None(
        AsyncFunction afterCompletion = null, 
        bool awaitCallback = true) 
        => new NoResult(afterCompletion, awaitCallback);
}
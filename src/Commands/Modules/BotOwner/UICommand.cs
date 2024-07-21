namespace Volte.Commands.Text.Modules;

public sealed partial class BotOwnerModule
{
    [Command("CreateUi", "Cui")]
    [Description("Create the ImGui UI on the machine running Volte.")]
    public Task<ActionResult> UiAsync(
        [Description("Desired font size of the UI.")] int fontSize = 14)
    {
        return VolteBot.TryCreateUi(Context.Services, fontSize, out var errorReason)
            ? None(() => Context.Message.AddReactionAsync(Emojis.BallotBoxWithCheck)) 
            : BadRequest($"Could not create UI thread: {errorReason}");
    }
}
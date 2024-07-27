using Volte.UI;

namespace Volte.Commands.Text.Modules;

public sealed partial class BotOwnerModule
{
    [Command("CreateUi", "Cui")]
    [Description("Create the ImGui UI on the machine running Volte.")]
    public Task<ActionResult> UiAsync(
        [Description("Desired font size of the UI.")] int fontSize = 17)
    {
        return UiManager.TryCreateUi(VolteBot.GetUiParams(fontSize), out var err)
            ? None(() => Context.Message.AddReactionAsync(Emojis.BallotBoxWithCheck)) 
            : BadRequest($"Could not create UI thread: {err?.Message}");
    }
}
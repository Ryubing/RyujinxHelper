using Volte.UI;

namespace Volte.Commands.Text.Modules;

public sealed partial class BotOwnerModule
{
    [Command("CreateUi", "Cui")]
    [Description("Create the ImGui UI on the machine running Volte.")]
    public Task<ActionResult> UiAsync(
        [Description("Desired font size of the UI.")] int fontSize = 17)
    {
        var uiParams = VolteBot.GetUiParams(fontSize);
        if (!UiManager.TryCreateUi(uiParams, out var err))
            return BadRequest($"Could not create UI thread: {err?.Message}");
        
        UiManager.AddView(new VolteUiView());
        UiManager.StartThread("Volte UI Thread");
        return None(() => Context.Message.AddReactionAsync(Emojis.BallotBoxWithCheck));

    }
}
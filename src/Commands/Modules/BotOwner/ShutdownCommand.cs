namespace Volte.Commands.Text.Modules;

public sealed partial class BotOwnerModule
{
    [Command("Shutdown")]
    [Description("Forces the bot to shutdown.")]
    public Task<ActionResult> ShutdownAsync()
        => Ok($"Goodbye! {Emojis.Wave}", async _ =>
        {
            await Task.Yield();
            Cts.Cancel();
        });
}
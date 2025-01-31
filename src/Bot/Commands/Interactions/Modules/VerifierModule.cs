using RyuBot.Interactions;

namespace RyuBot.Commands.Interactions.Modules;

[RequireRyubingGuildPrecondition]
public partial class VerifierModule : RyujinxBotSlashCommandModule
{
    public VerifierService Verifier { get; set; }
}
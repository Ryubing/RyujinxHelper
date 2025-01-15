using RyuBot.Interactions;

namespace RyuBot.Commands.Interactions.Modules;

public partial class CompatibilityModule : RyujinxBotSlashCommandModule
{
    public CompatibilityCsvService Compatibility { get; set; }
}
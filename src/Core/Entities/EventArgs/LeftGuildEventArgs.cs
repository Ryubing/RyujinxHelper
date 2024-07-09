namespace Volte.Core.Entities;

public sealed class LeftGuildEventArgs : EventArgs
{
    public SocketGuild Guild { get; }

    public LeftGuildEventArgs(SocketGuild guild) => Guild = guild;
}
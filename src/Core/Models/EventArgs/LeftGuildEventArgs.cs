using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Gommon;

namespace Volte.Core.Models.EventArgs
{
    public sealed class LeftGuildEventArgs : System.EventArgs
    {
        public LeftGuildEventArgs(SocketGuild guild)
        {
            Guild = guild.Cast<SocketGuild>();
        }

        public SocketGuild Guild { get; }
    }
}
namespace RyuBot.Entities;

public class MessageReceivedEventArgs
{
    public SocketUserMessage Message { get; }
    public IServiceProvider Provider { get; }

    public MessageReceivedEventArgs(SocketMessage s, IServiceProvider provider)
    {
        Message = s.Cast<SocketUserMessage>() ?? throw new ArgumentException($"{nameof(s)} is not a SocketUserMessage; aborting EventArgs construction.");
        Provider = provider;
    }
}
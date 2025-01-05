using Discord.Interactions;

namespace RyuBot.Interactions.Results;

public class BadRequestResult : RuntimeResult
{
    public bool DidDefer { get; init; }

    public BadRequestResult(string error, bool didDefer) : base(null, error)
    {
        DidDefer = didDefer;
    }

    public static implicit operator Task<RuntimeResult>(BadRequestResult input) 
        => Task.FromResult<RuntimeResult>(input);
}
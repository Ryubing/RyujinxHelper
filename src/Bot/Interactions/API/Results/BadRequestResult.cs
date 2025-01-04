using Discord.Interactions;

namespace RyuBot.Interactions.Results;

public class BadRequestResult : RuntimeResult
{
    public BadRequestResult(string error) : base(null, error) {}

    public static implicit operator Task<BadRequestResult>(BadRequestResult input) 
        => Task.FromResult(input);
}
namespace Volte;

public static class Program
{
    private static async Task Main(string[] args)
    {
        if (UnixHelper.TryParseNamedArguments(args.JoinToString(' '), out var output))
            await VolteBot.StartAsync(output.Parsed);
        else
        {
            Error(output.Error);
            await VolteBot.StartAsync([]);
        }
    }
}
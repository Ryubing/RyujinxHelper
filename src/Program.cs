namespace Volte;

public static class Program
{
    static async Task Main(string[] args)
    {
        await VolteBot.StartAsync(args.ContainsIgnoreCase("--ui"));
    }
}
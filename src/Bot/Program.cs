using Qommon.Collections;

namespace Volte;

public static class Program
{
    public static ReadOnlyDictionary<string, string> CommandLineArguments { get; private set; }
    
    private static async Task Main(string[] args)
    {
        if (!UnixHelper.TryParseNamedArguments(args, out var output))
            if (output.Error is not InvalidOperationException)
                Error(output.Error);

        await Main(output.Parsed);
    }
    
    private static async Task Main(Dictionary<string, string> args)
    {
        CommandLineArguments = new ReadOnlyDictionary<string, string>(args ?? new Dictionary<string, string>());
        await VolteBot.StartAsync();
    }
}
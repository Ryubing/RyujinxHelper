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

        CommandLineArguments = new ReadOnlyDictionary<string, string>(output.Parsed ?? new Dictionary<string, string>());
        await VolteBot.StartAsync();
    }
}
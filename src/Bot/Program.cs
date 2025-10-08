using System.Collections.ObjectModel;

namespace RyuBot;

public static class Program
{
    public static ReadOnlyDictionary<string, string> CommandLineArguments { get; private set; }
    
    private static async Task Main(string[] args)
    {
        //this is the entrypoint for command-line, so log to the console. the UI will have its own log.
        OutputLogToStandardOut(); 
        
        if (!UnixHelper.TryParseNamedArguments(args, out var output))
            if (output.Error is not InvalidOperationException)
                Error(output.Error);

        await StartBotAsync(output.Parsed);
    }
    
    public static async Task StartBotAsync(Dictionary<string, string> args = null)
    {
        CommandLineArguments = new (args ?? new Dictionary<string, string>());
        await RyujinxBot.StartAsync();
    }
}
using Qmmands;
using RyuBot.Commands.Text;

namespace RyuBot.Services;

public sealed class MessageService : BotService
{
    private readonly CommandService _commandService;
        
    public MessageService(CommandService commandService)
    {
        _commandService = commandService;
    }

    public async Task HandleMessageAsync(MessageReceivedEventArgs args)
    {
        List<string> prefixes = [
            $"<@{args.Context.Client.CurrentUser.Id}> ",
            $"<@!{args.Context.Client.CurrentUser.Id}> "
        ];

        if (CommandUtilities.HasAnyPrefix(args.Message.Content, prefixes, StringComparison.OrdinalIgnoreCase, out _,
                out var cmd))
        {
            await _commandService.ExecuteAsync(cmd, args.Context);
        }
    }
    
    internal static IEnumerable<Type> AddTypeParsers(CommandService service)
    {
        var parsers = Assembly.GetExecutingAssembly().ExportedTypes
            .Where(x => x.HasAttribute<InjectTypeParserAttribute>())
            .ToList();

        var csMirror = Mirror.Reflect(service);

        foreach (var parser in parsers)
        {
            csMirror.CallGeneric("AddTypeParser", 
                parser.BaseType!.GenericTypeArguments,
                BindingFlags.Public,
                args: [
                    parser.GetConstructor(Type.EmptyTypes)?.Invoke([]) ?? throw new InvalidOperationException($"Couldn't find no-args constructor for {parser.AsFullNamePrettyString()}"), 
                    parser.GetCustomAttribute<InjectTypeParserAttribute>()!.OverridePrimitive 
                ]
            );
                
            yield return parser;
        }
    }
}
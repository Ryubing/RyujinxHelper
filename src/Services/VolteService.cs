namespace Volte.Services;

/// <summary>
///     Base for every Volte service, discoverable by the RegisterEventHandlers extension method.
/// </summary>
public abstract class VolteService
{
    private static readonly List<Func<DiscordSocketClient, Task>> _eventHandlerRegisters = [];

    public static Task RegisterClientAsync(DiscordSocketClient client) 
        => _eventHandlerRegisters.Count < 1 
            ? Task.CompletedTask 
            : Task.WhenAll(_eventHandlerRegisters.Select(eventFunc => eventFunc(client)))
                .Apply(_ => _eventHandlerRegisters.Clear());
    

    protected void EventHandlers(Func<DiscordSocketClient, Task> registrationFunc) 
        => _eventHandlerRegisters.Add(registrationFunc);
}
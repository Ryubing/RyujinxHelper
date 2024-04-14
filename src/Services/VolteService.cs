﻿namespace Volte.Services;

/// <summary>
///     Base for every Volte service, discoverable by the RegisterEventHandlers extension method.
/// </summary>
public interface IVolteService;

public abstract class VolteExtension : IVolteService
{
    public abstract Task OnInitializeAsync(DiscordSocketClient client);
}
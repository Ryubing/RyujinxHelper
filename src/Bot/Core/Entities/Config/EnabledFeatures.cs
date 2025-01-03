using System.Text.Json.Serialization;

namespace RyuBot.Entities;

/// <summary>
///     Model that represents enabled/disabled features as defined in your config.
/// </summary>
public sealed class EnabledFeatures
{
    [JsonPropertyName("log_to_file")]
    public bool LogToFile { get; set; } = true;
}
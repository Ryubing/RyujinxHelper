using System.Text.Json.Serialization;

namespace RyuBot.Entities;

public sealed class GuildExtras
{
    internal GuildExtras()
    {
        SelfRoles = [];
        Tags = [];
        Warns = [];
    }
        
    [JsonPropertyName("mod_log_case_number")]
    public ulong ModActionCaseNumber { get; set; }
        
    [JsonPropertyName("auto_parse_quote_urls")]
    public bool AutoParseQuoteUrls { get; set; }

    [JsonPropertyName("self_roles")]
    public HashSet<ulong> SelfRoles { get; set; }

    [JsonPropertyName("tags")]
    public HashSet<Tag> Tags { get; set; }

    [JsonPropertyName("warns")]
    public HashSet<Warn> Warns { get; set; }
    
    public override string ToString()
        => JsonSerializer.Serialize(this, Config.JsonOptions);
}
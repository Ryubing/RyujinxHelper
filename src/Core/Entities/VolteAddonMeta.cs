using System.Text.Json.Serialization;

namespace Volte.Core.Entities
{
    public class VolteAddonMeta
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public class VolteAddon
    {
        public VolteAddonMeta Meta { get; init; }
        public string Script { get; init; }
    }
}
using System.Text.Json.Serialization;

namespace RyuBot.Services;

public readonly struct ReleaseChannels
{
    internal ReleaseChannels(ReleaseChannelPair channelPair)
    {
        Stable = new Channel(channelPair.Stable);
        Canary = new Channel(channelPair.Canary);
    }

    public readonly Channel Stable;
    public readonly Channel Canary;
        
    public readonly struct Channel
    {
        public Channel(string raw)
        {
            var parts = raw.Split('/');
            Owner = parts[0];
            Repo = parts[1];
        }
            
        public readonly string Owner;
        public readonly string Repo;

        public override string ToString() => $"{Owner}/{Repo}";

        public string GetLatestReleaseApiUrl() =>
            $"https://api.github.com/repos/{ToString()}/releases/latest";
    }
}
    
[JsonSerializable(typeof(ReleaseChannelPair))]
partial class ReleaseChannelPairContext : JsonSerializerContext;

internal class ReleaseChannelPair
{
    [JsonPropertyName("stable")]
    public string Stable { get; set; }
    [JsonPropertyName("canary")]
    public string Canary { get; set; }
}
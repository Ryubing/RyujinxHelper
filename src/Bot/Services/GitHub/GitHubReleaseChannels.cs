using System.Text.Json.Serialization;
using Octokit;

namespace RyuBot.Services;

public readonly struct GitHubReleaseChannels
{
    internal GitHubReleaseChannels(ReleaseChannelPair channelPair)
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

        public Task<Release> GetLatestReleaseAsync(GitHubClient ghc)
            => ghc.Repository.Release.GetLatest(Owner, Repo);

        public Task<Release> GetReleaseAsync(GitHubClient ghc, string tag)
            => ghc.Repository.Release.Get(Owner, Repo, tag);
    }
}
    
[JsonSerializable(typeof(ReleaseChannelPair))]
internal partial class ReleaseChannelPairContext : JsonSerializerContext;

internal class ReleaseChannelPair
{
    [JsonPropertyName("stable")]
    public string Stable { get; set; }
    [JsonPropertyName("canary")]
    public string Canary { get; set; }
}
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace RyuBot.Services;

[JsonSerializable(typeof(GitLabReleaseChannels))]
internal partial class GitLabReleaseChannelPairContext : JsonSerializerContext;

public class GitLabReleaseChannels
{
    public static async Task<GitLabReleaseChannels> GetAsync(HttpClient httpClient)
        => await httpClient.GetFromJsonAsync(
            "https://git.ryujinx.app/ryubing/ryujinx/-/snippets/1/raw/main/meta.json",
            GitLabReleaseChannelPairContext.Default.GitLabReleaseChannels);

    [JsonPropertyName("stable")] public ChannelType Stable { get; set; }
    [JsonPropertyName("canary")] public ChannelType Canary { get; set; }

    public class ChannelType
    {
        [JsonPropertyName("id")] public long Id { get; set; }
        [JsonPropertyName("group")] public string Group { get; set; }
        [JsonPropertyName("project")] public string Project { get; set; }

        public override string ToString() => $"{Group}/{Project}";

        public Task<GitLabReleaseJsonResponse> GetLatestReleaseAsync(HttpClient httpClient)
            => GitLabApi.GetLatestReleaseAsync(httpClient, Id);

        public Task<GitLabReleaseJsonResponse> GetReleaseAsync(HttpClient httpClient, string tagName)
            => GitLabApi.GetReleaseAsync(httpClient, Id, tagName);
    }
}

using System.Net.Http.Headers;
using System.Text.Json.Serialization.Metadata;

namespace RyuBot.Services;

public static class GitLabApi
{
    private static readonly GitLabReleaseJsonResponseSerializerContext ReleaseSerializerContext =
        new();

    public static Task<GitLabReleaseJsonResponse> GetLatestReleaseAsync(HttpClient httpClient, long projectId) 
        => GetReleaseAsync(httpClient, projectId, "permalink/latest");

    public static Task<GitLabReleaseJsonResponse> GetReleaseAsync(HttpClient httpClient, long projectId, string tagName) =>
        httpClient.ReadContentAsJsonAsync(
            CreateRequest(HttpMethod.Get,
                $"{Config.GitLabAuth.InstanceUrl.TrimEnd('/')}/api/v4/projects/{projectId}/releases/{tagName}"),
            ReleaseSerializerContext.GitLabReleaseJsonResponse
        );

    private static HttpRequestMessage CreateRequest(HttpMethod method, string uri)
        => new HttpRequestMessage(method, uri).Apply(m =>
        {
            m.Headers.Authorization = AuthenticationHeaderValue.Parse($"Bearer {Config.GitLabAuth.AccessToken}");
        });

    public static async Task<T> ReadContentAsJsonAsync<T>(
        this HttpClient httpClient,
        HttpRequestMessage message,
        JsonTypeInfo<T> typeInfo)
    {
        var response = await httpClient.SendAsync(message);

        return JsonSerializer.Deserialize(await response.Content.ReadAsStringAsync(), typeInfo);
    }
}
#nullable enable
using System.Text.Json.Serialization;

namespace RyuBot.Services.Forgejo.Models;

/// <summary>Represents a user on Forgejo</summary>
public class ForgejoUser
{
    [JsonPropertyName("active")] public bool IsActive { get; set; }
    [JsonPropertyName("avatar_url")] public string AvatarUrl { get; set; }
    [JsonPropertyName("created")] public DateTimeOffset CreatedAt { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("email")] public string Email { get; set; }
    [JsonPropertyName("followers_count")] public long FollowersCount { get; set; }
    [JsonPropertyName("following_count")] public long FollowingCount { get; set; }
    [JsonPropertyName("full_name")] public string FullName { get; set; }
    [JsonPropertyName("html_url")] public string HtmlUrl { get; set; }
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("is_admin")] public bool IsAdmin { get; set; }
    [JsonPropertyName("language")] public string? Language { get; set; }
    [JsonPropertyName("last_login")] public DateTimeOffset? LastLogin { get; set; }
    [JsonPropertyName("location")] public string? Location { get; set; }
    [JsonPropertyName("login")] public string? Username { get; set; }
    [JsonPropertyName("login_name")] public string? LoginUsername { get; set; }
    [JsonPropertyName("prohibit_login")] public bool ProhibitedLogin { get; set; }
    [JsonPropertyName("pronouns")] public string? Pronouns { get; set; }
    [JsonPropertyName("restricted")] public bool Restricted { get; set; }
    [JsonPropertyName("source_id")] public long? SourceId { get; set; }
    [JsonPropertyName("starred_repos_count")] public long StarredReposCount { get; set; }
    [JsonPropertyName("visibility")] public string Visibility { get; set; }
    [JsonPropertyName("website")] public string? Website { get; set; }

    public static bool IsNameTaken(ForgejoUser[] users, string username) 
        => users.Any(x => x.Username == username);

    public static bool IsEmailAlreadyRegistered(ForgejoUser[] users, string email)
        => users.Any(x => x.Email == email);
}

[JsonSerializable(typeof(IEnumerable<ForgejoUser>))]
[JsonSerializable(typeof(ForgejoUser[]))]
internal partial class ForgejoUserSerializerContext : JsonSerializerContext;
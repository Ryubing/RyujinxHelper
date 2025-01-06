namespace RyuBot.Entities;

public record GuildConfig(ulong GuildId, ulong PirateRoleId, string RepoOwner, string RepoName);

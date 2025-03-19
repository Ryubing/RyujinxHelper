namespace RyuBot.Entities;

public record GitLabAuth(string AccessToken, string InstanceUrl = "https://git.ryujinx.app");
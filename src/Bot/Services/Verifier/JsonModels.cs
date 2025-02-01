using System.Text.Json.Serialization;

namespace RyuBot.Services;


public class HashActionResponse
{
    [JsonPropertyName("result")]
    public int InternalResult { get; set; }

    public ResultCode Result => (ResultCode)InternalResult;
    
    [JsonPropertyName("hash")]
    public string Hash { get; set; }

}

public class HashActionRequest
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = "hash";
    
    [JsonPropertyName("id")]
    public ulong Id { get; set; }
}

public class VerifyActionResponse
{
    [JsonPropertyName("result")]
    public int InternalResult { get; set; }

    public ResultCode Result => (ResultCode)InternalResult;
}

public class VerifyActionRequest
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = "verify";
    
    [JsonPropertyName("id")]
    public ulong Id { get; set; }
    
    [JsonPropertyName("token")]
    public string Token { get; set; }
}
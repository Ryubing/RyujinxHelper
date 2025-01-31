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
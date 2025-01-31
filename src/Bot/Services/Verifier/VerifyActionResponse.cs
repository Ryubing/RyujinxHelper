using System.Text.Json.Serialization;

namespace RyuBot.Services;

public class VerifyActionResponse
{
    [JsonPropertyName("result")]
    public int InternalResult { get; set; }

    public ResultCode Result => (ResultCode)InternalResult;
}
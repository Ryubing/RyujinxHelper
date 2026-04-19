using System.Text.Json.Serialization;

namespace RyuBot.Entities;

public class AccountRequest
{
    [JsonPropertyName("requestor_id")] public ulong Requestor { get; set; }
    [JsonPropertyName("desired_username")] public string DesiredUsername { get; set; }
    [JsonPropertyName("email")] public string Email { get; set; }
    [JsonPropertyName("reason_for_request")] public string Reason { get; set; }
}
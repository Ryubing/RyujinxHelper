using Optional = Gommon.Optional;

namespace RyuBot.Services;

public class VerifierService : BotService
{
    public const string ApiBaseUrl = "https://switch.lotp.it/verifier.php";

    private readonly HttpClient _httpClient;

    public VerifierService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool Success, Gommon.Optional<string> Hash)> GetHashAsync(ulong userId)
    {
        var hashResponse = JsonSerializer.Deserialize<HashActionResponse>(
            await _httpClient.GetStringAsync($"{ApiBaseUrl}?action=hash&id={userId}")
        );

        return (
            hashResponse.Result is ResultCode.Success,
            hashResponse.Hash is "-1"
                ? Gommon.Optional<string>.None
                : hashResponse.Hash
        );
    }
    
    public async Task<ResultCode> VerifyAsync(ulong userId, string token)
    {
        var hashResponse = JsonSerializer.Deserialize<VerifyActionResponse>(
            await _httpClient.GetStringAsync($"{ApiBaseUrl}?action=verify&id={userId}&token={token}")
        );

        return hashResponse.Result;
    }
}
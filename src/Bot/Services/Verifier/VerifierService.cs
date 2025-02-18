﻿using System.Net.Http.Json;
using Optional = Gommon.Optional;

namespace RyuBot.Services;

public class VerifierService : BotService
{
    public const string ApiBaseUrl = "https://switch.lotp.it/verifier.php";

    private readonly HttpClient _httpClient;
    private readonly DiscordSocketClient _client;

    public VerifierService(HttpClient httpClient, DiscordSocketClient client)
    {
        _httpClient = httpClient;
        _client = client;
    }

    public async Task<(ResultCode Result, Gommon.Optional<string> Hash)> GetHashAsync(ulong userId)
    {
        var hashRequest = new HashActionRequest { Id = userId };

        var response = await _httpClient.PostAsJsonAsync(ApiBaseUrl, hashRequest);

        var hashResponse = JsonSerializer.Deserialize<HashActionResponse>(
            await response.Content.ReadAsStringAsync()
        );

        return (
            hashResponse.Result,
            hashResponse.Hash is "-1"
                ? Gommon.Optional<string>.None
                : hashResponse.Hash
        );
    }

    public async Task<VerifyActionResponse> VerifyAsync(ulong userId, string token)
    {
        var verifyRequest = new VerifyActionRequest { Id = userId, Token = token };

        var response = await _httpClient.PostAsJsonAsync(ApiBaseUrl, verifyRequest);

        return JsonSerializer.Deserialize<VerifyActionResponse>(
            await response.Content.ReadAsStringAsync()
        );
    }

    public async Task SendVerificationModlogMessageAsync(SocketGuildUser member, VerifyActionResponse response)
    {
        if (await _client.GetChannelAsync(1318250869980004394) is not ITextChannel channel) return;

        await embed().SendToAsync(channel);

        return;

        EmbedBuilder embed() => new EmbedBuilder()
            .WithColor(Config.SuccessColor)
            .WithTitle(response.Result is ResultCode.Success
                ? "Verification success"
                : "Verification failed")
            .WithAuthor(member)
            .AddField(
                response.Result is ResultCode.Success
                    ? "Verification Place #"
                    : "Error",
                response.Result is ResultCode.Success
                    ? member.Guild.Users.Count(u => u.HasRole(1334992661198930001))
                        .Into(count => $"{count.ToOrdinalWords(WordForm.Abbreviation)} ({count})")
                    : $"{Enum.GetName(response.Result)} ({(int)response.Result})"
            );
    }
    
    public async Task SendVerificationModlogErrorMessageAsync(string command, SocketGuildUser member, Exception e)
    {
        if (await _client.GetChannelAsync(1318250869980004394) is not ITextChannel channel) return;

        await channel.SendMessageAsync("<@&1337959521833713715>", embed: embed().Build());

        return;

        EmbedBuilder embed() => new EmbedBuilder()
            .WithColor(Config.SuccessColor)
            .WithAuthor(member)
            .WithTitle($"{e.GetType().AsPrettyString()} thrown in '{command}'")
            .WithDescription($"{e.Message}\n\n{e.StackTrace}");
    }
}
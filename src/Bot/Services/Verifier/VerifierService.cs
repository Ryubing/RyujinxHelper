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

    public async Task<int> SendVerificationResponseCompletedMessagesAsync(SocketGuildUser member, VerifyActionResponse response, bool isInvokedBeforeRoleAdd)
    {
        var verifiedMemberCount = member.Guild.Users.Count(u => u.HasRole(1334992661198930001));

        if (isInvokedBeforeRoleAdd)
            verifiedMemberCount++;
        
        await SendVerificationWelcomeMessageAsync(member, response, verifiedMemberCount);
        await SendVerificationModlogMessageAsync(member, response, verifiedMemberCount);

        return verifiedMemberCount;
    }
    
    private async Task SendVerificationWelcomeMessageAsync(SocketGuildUser member, VerifyActionResponse response, int verifiedMemberCount)
    {
        if (response.Result is not ResultCode.Success) return;
        if (await _client.GetChannelAsync(1337187108002992140) is not ITextChannel channel) return;

        await channel.SendMessageAsync($"# Verified Switch Owner #{verifiedMemberCount}\n\nWelcome {member.Mention} to the verified club!");
    }

    private async Task SendVerificationModlogMessageAsync(SocketGuildUser member, VerifyActionResponse response, int verifiedMemberCount)
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
                    ? $"{verifiedMemberCount.ToOrdinalWords(WordForm.Abbreviation)} ({verifiedMemberCount})"
                    : $"{Enum.GetName(response.Result)} ({response.InternalResult})"
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
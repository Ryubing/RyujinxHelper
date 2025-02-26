﻿using Discord.Interactions;

namespace RyuBot.Commands.Interactions.Modules;

public partial class VerifierModule
{
    [SlashCommand("verify", "Verifies your modded Switch ownership via the Switch Verifier Homebrew.")]
    public async Task<RuntimeResult> VerifyAsync(
        [Summary(description: "The token generated by the Switch Verifier homebrew, after being given your hash.")] 
        string token)
    {
        if (Context.User is not SocketGuildUser member) return None();
        
        await DeferAsync(true);

        try
        {
            var response = await Verifier.VerifyAsync(Context.User.Id, token);

            var verifiedMemberCount = await Verifier.SendVerificationResponseCompletedMessagesAsync(member, response);

            return response.Result switch
            {
                ResultCode.Success => Ok(String(sb =>
                {
                    sb.AppendLine("Success! You can now get help, and you can chat in <#1337187108002992140>.");
                    sb.Append($"You are the {verifiedMemberCount.ToOrdinalWords(WordForm.Abbreviation)} ({verifiedMemberCount}) user to be verified for Switch ownership.");
                }), () => member.AddRoleAsync(1334992661198930001)),
                ResultCode.InvalidInput or ResultCode.TokenIsZeroes => BadRequest("An input value was invalid."),
                ResultCode.InvalidTokenLength => BadRequest(String(sb =>
                {
                    sb.AppendLine("The provided token didn't match the expected length.");
                    sb.Append("If you just input what you got from `get-hash` then you didn't read how to use this properly.");
                })),
                ResultCode.ExpiredToken => BadRequest(String(sb =>
                {
                    sb.AppendLine("The provided token has expired.");
                    sb.Append(
                        "If you just made the token, please be sure your Switch's time & time zone matches reality.");
                })),
                _ => BadRequest("Invalid token.")
            };
        }
        catch (Exception e)
        {
            await Verifier.SendVerificationModlogErrorMessageAsync("verify", member, e);
            return BadRequest(
                "An internal error occurred when processing this command. It has been forwarded to the developer.");
        }
    }
}
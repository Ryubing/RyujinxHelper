using System.Text.RegularExpressions;
using Discord.Interactions;
using RyuBot.Interactions;
using RyuBot.Services.Forgejo.Models;

namespace RyuBot.Commands.Interactions.Modules;

public partial class ForgejoModule
{
    public DataService Persistency { get; set; }
    
    
    public const ulong RequestFeedId = 1495304560808296559;

    [SlashCommand("request-account", "Request an account on the Ryubing Forgejo. Only usable once.")]
    [RequireRyubingGuildPrecondition]
    public async Task<RuntimeResult> RequestAccountAsync()
    {
        if (!(Config.EnabledFeatures?.AccountRequesting ?? false))
        {
            return BadRequest(
                "Account requesting has been disabled by the bot administrator. Most likely reason is server maintenance.");
        }

        if (Persistency.HasAlreadyRequestedAccount(Context.User.Id))
        {
            return BadRequest("You cannot request another account.");
        }

        if (IsInGuild())
        {
            var requestingUser = await Context.Guild.HardCast<IGuild>().GetUserAsync(Context.User.Id);

            if (requestingUser.RoleIds.Any(x => x is 1488287503872954378)) // Forgejo User role
            {
                return BadRequest("You cannot request another account.");
            }
        }

        return Ok(modal => modal
            .WithTitle("Request Account on our Forgejo")
            .WithCustomId(new MessageComponentId("account", "request", Context.User.Id))
            .AddTextInput("What is your email?", "email_input")
            .AddTextInput("What username would you like?", "username_input")
            .AddTextInput("Why are you requesting an account?", "reason", TextInputStyle.Paragraph,
                minLength: "I want to contribute".Length)
        );
    }

    public static async Task RespondToAccountRequestModalAsync(SocketModal modal, MessageComponentId customId,
        IServiceProvider provider)
    {
        await modal.DeferAsync(true);

        var forgejo = provider.Get<ForgejoService>();
        var persistency = provider.Get<DataService>();
        var requestorId = customId.Value!.Parse<ulong>();

        if (persistency.HasAlreadyRequestedAccount(requestorId))
        {
            await badRequest("You cannot request another account.").ExecuteAsync();
            return;
        }

        var guild = RyujinxBot.Client.GetGuild(modal.GuildId ?? 1294443224030511104);
        var requestingUser = await guild.HardCast<IGuild>().GetUserAsync(requestorId);

        if (requestingUser.RoleIds.Any(x => x is 1488287503872954378)) // Forgejo User role
        {
            await badRequest("You cannot request another account.").ExecuteAsync();
            return;
        }

        var err = ExtractData(modal, out var data);
        if (err != null)
        {
            await badRequest(err).ExecuteAsync();
            return;
        }

        if (forgejo.IsEmailAlreadyRegistered(data.Email))
        {
            await badRequest(
                    $"The email address `{data.Email}` is already registered to a Forgejo user. If this is your email, click 'Forgot password' on the Forgejo sign in page then check your inbox.")
                .ExecuteAsync();
            return;
        }

        if (forgejo.IsNameTaken(data.Username))
        {
            await badRequest($"The name `{data.Username}` is taken. Please try another one.").ExecuteAsync();
            return;
        }

        await modal.CreateReply(ephemeral: true, deferred: true)
            .WithEmbed(e => e
                .WithColor(Color.Gold)
                .WithTitle("Requested account!")
                .WithCurrentTimestamp()
            ).ExecuteAsync();

        persistency.RegisterAccountRequest(requestorId, data.Email, data.Username, data.Reason);

        await guild.GetTextChannel(RequestFeedId)
            .SendMessageAsync(embed: new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithTitle("Account Request")
                    .WithAuthor(requestingUser.Username, requestingUser.GetEffectiveAvatarUrl(), $"https://discord.com/users/{requestorId}")
                    .AddField("Username", data.Username, true)
                    .AddField("Email", Format.Spoiler(data.Email), true)
                    .AddField("Reason for requesting account", Format.Code(data.Reason, string.Empty)).Build(),
                components: new ComponentBuilder()
                    .AddActionRows(new ActionRowBuilder(
                        Buttons.Success(new MessageComponentId("account", "request", requestorId, "accept"), "Create Account", Emote.Parse("<:AnubisHappy:1489462922676535376>")),
                        Buttons.Danger(new MessageComponentId("account", "request", requestorId, "deny"), "Reject Request", Emote.Parse("<:AnubisReject:1489462838362640556>"))
                    ))
                    .Build()
            );

        ReplyBuilder<SocketModal> badRequest(string error)
        {
            return modal.CreateReply(ephemeral: true, deferred: true)
                .WithEmbed(e => e
                    .WithColor(Color.Red)
                    .WithTitle("No can do, partner.")
                    .WithDescription(error)
                    .WithCurrentTimestamp()
                );
        }
    }
    
    public static async Task RespondToJudgementAsync(SocketMessageComponent component, MessageComponentId customId,
        IServiceProvider provider)
    {
        await component.DeferAsync();

        var forgejo = provider.Get<ForgejoService>();
        var persistency = provider.Get<DataService>();
        var requestorId = customId.Value!.Parse<ulong>();
        var guild = RyujinxBot.Client.GetGuild(component.GuildId ?? 1294443224030511104);
        var requestingUser = await guild.HardCast<IGuild>().GetUserAsync(requestorId);

        if (customId.TrailingContent is "deny")
        {
            persistency.RedactAccountRequestFor(requestorId);
            await component.CreateReply(deferred: true)
                .WithEmbeds(component.Message.Embeds.First().ToEmbedBuilder()
                    .WithColor(Color.Red)
                    .WithFooter("This account request has been denied!")
                )
                .WithNoActionRows()
                .ExecuteAsync();
        } 
        else if (customId.TrailingContent is "accept")
        {
            if (persistency.GetAccountRequestFor(requestorId, out var request))
            {
                var (createdUser, error) = await forgejo.CreateUserAsync(request.DesiredUsername, request.Email);

                if (error != null)
                {
                    Error(error);
                    await component.CreateReply(deferred: true)
                        .WithEmbeds(component.Message.Embeds.First().ToEmbedBuilder()
                            .WithColor(Color.Red)
                            .WithFooter("Failed to create Forgejo account. Check console for more details.")
                        )
                        .ExecuteAsync();
                }
                else
                {
                    await component.CreateReply(deferred: true)
                        .WithEmbeds(component.Message.Embeds.First().ToEmbedBuilder()
                            .WithColor(Color.Green)
                            .WithUrl($"{Config.ForgejoAuth.InstanceUrl.TrimEnd('/')}/admin/users/{createdUser.id!.Value}")
                            .WithFooter("This request was granted.")
                        )
                        .WithNoActionRows()
                        .ExecuteAsync();

                    persistency.RemoveAccountRequestFor(requestorId);

                    await requestingUser.AddRoleAsync(1488287503872954378); // Forgejo User role
                }
            }
            else
            {
                await component.Message.TryDeleteAsync();
            }
        }
    }

    private static string? ExtractData(SocketModal modal, out (string Email, string Username, string Reason) result)
    {
        result = default;
        result.Email = modal.Data.Components.First(x => x.CustomId == "email_input").Value;

        if (!EmailPattern.IsMatch(result.Email))
            return
                "Input email does not look like an email; contact the administrator if you know for certain it is a real email.";

        result.Username = modal.Data.Components.First(x => x.CustomId == "username_input").Value;
        result.Reason = modal.Data.Components.First(x => x.CustomId == "reason").Value;

        return null;
    }

    private static readonly Regex EmailPattern = EmailRegex();

    [GeneratedRegex(
        """(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])""",
        RegexOptions.ECMAScript | RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex EmailRegex();
}
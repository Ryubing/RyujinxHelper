using Discord.Interactions;

using RyuBot.Interactions;

namespace Volte.Interactions.Commands.Modules;

public partial class GitHubModule
{
    [SlashCommand("latest", "Show the download URLs for the latest release of Ryujinx.")]
    public async Task<RuntimeResult> LatestReleaseAsync(
        [Summary("release_channel", "The release channel to look for the latest version from. Only has more options in Ryubing.")]
        [Autocomplete<LatestReleaseAutocompleter>]
        string releaseChannel = "Stable")
    {
        if (releaseChannel is not ("Stable" or "Canary"))
            return BadRequest(
                "Unknown release channel. Please wait for the autocomplete suggestions to fill in if you aren't sure what to put!");
        
        var isCanary = releaseChannel.EqualsIgnoreCase("Canary");
        
        var latest = releaseChannel.EqualsIgnoreCase("Canary") && Context.Guild?.Id == 1294443224030511104
            ? await GitHub.GetLatestCanaryAsync()
            : await GitHub.GetLatestStableAync(Context);

        var assets = latest.Assets.Where(x =>
            !x.Name.ContainsIgnoreCase("nogui") && !x.Name.ContainsIgnoreCase("headless"))
            .ToArray();

        var windows = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("win_x64"));
        var linuxX64 = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("linux_x64") && !x.Name.EndsWithIgnoreCase(".AppImage"));
        var linuxX64AppImage = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("x64") && x.Name.EndsWithIgnoreCase(".AppImage"));
        var macOs = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("macos_universal"));
        var linuxArm64 = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("linux_arm64") && !x.Name.EndsWithIgnoreCase(".AppImage"));
        var linuxArm64AppImage = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("arm64") && x.Name.EndsWithIgnoreCase(".AppImage"));

        StringBuilder releaseBody = new();

        if (windows != null)
            releaseBody.AppendLine($"{Format.Url(windows.Name, windows.BrowserDownloadUrl)}");
        
        if (linuxX64 != null)
            releaseBody.Append($"{Format.Url(linuxX64.Name, linuxX64.BrowserDownloadUrl)} ");

        if (linuxX64AppImage != null)
            releaseBody.AppendLine($"({Format.Url("AppImage", linuxX64AppImage.BrowserDownloadUrl)})");
        else
            releaseBody.AppendLine();
        
        if (macOs != null)
            releaseBody.AppendLine($"{Format.Url(macOs.Name, macOs.BrowserDownloadUrl)}");
        
        if (linuxArm64 != null)
            releaseBody.Append($"{Format.Url(linuxArm64.Name, linuxArm64.BrowserDownloadUrl)} ");
        
        if (linuxArm64AppImage != null)
            releaseBody.AppendLine($"({Format.Url("AppImage", linuxArm64AppImage.BrowserDownloadUrl)})");
        else
            releaseBody.AppendLine();
            

        return Ok(Context.CreateReplyBuilder()
            .WithEmbed(embed =>
            {
                embed.WithTitle($"Ryujinx {(!isCanary ? "Stable" : string.Empty)} {latest.Name}");
                embed.AddField("Commit SHA", latest.TargetCommitish);
                embed.WithDescription(releaseBody.ToString());
                embed.WithTimestamp(latest.CreatedAt);
            }));
    }
}

public class LatestReleaseAutocompleter : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context, 
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter, 
        IServiceProvider services)
    {
        if (!autocompleteInteraction.Data.Options.Any(option => option.Focused))
            return Task.FromResult(AutocompletionResult.FromSuccess());
        
        
        List<AutocompleteResult> result = [new("Stable", "Stable")];

        if (context.Guild?.Id == 1294443224030511104)
        {
            result.Add(new AutocompleteResult("Canary", "Canary"));
        }

        return Task.FromResult(AutocompletionResult.FromSuccess(result));
    }
}
using Discord.Interactions;
using Octokit;
using RyuBot.Commands.Interactions;
using RyuBot.Interactions;

namespace RyuBot.Commands.Interactions.Modules;

public partial class GitHubModule
{
    [SlashCommand("latest", "Show the download URLs for the latest release of Ryujinx.")]
    [RequireNotPiratePrecondition]
    [RequireRyubingGuildPrecondition]
    public async Task<RuntimeResult> LatestReleaseAsync(
        [Summary("release_channel", "The release channel to look for the latest version from. Only has more options in Ryubing.")]
        [Autocomplete<LatestReleaseAutocompleter>]
        string releaseChannel = "Stable")
    {
        if (releaseChannel is not ("Stable" or "Canary"))
            return BadRequest(
                "Unknown release channel. Please wait for the autocomplete suggestions to fill in if you aren't sure what to put!");
        
        var isCanary = releaseChannel.EqualsIgnoreCase("Canary");

        var latest = isCanary
            ? await GitHub.GetLatestCanaryAsync()
            : await GitHub.GetLatestStableAsync();

        var assets = latest.Assets.Where(x =>
            !x.Name.ContainsIgnoreCase("nogui") && !x.Name.ContainsIgnoreCase("headless")
        ).ToArray();

        var windowsX64 = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("win_x64"));
        var windowsArm64 = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("win_arm64"));
        var linuxX64 = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("linux_x64") && !x.Name.EndsWithIgnoreCase(".AppImage"));
        var linuxX64AppImage = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("x64") && x.Name.EndsWithIgnoreCase(".AppImage"));
        var macOs = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("macos_universal"));
        var linuxArm64 = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("linux_arm64") && !x.Name.EndsWithIgnoreCase(".AppImage"));
        var linuxArm64AppImage = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("arm64") && x.Name.EndsWithIgnoreCase(".AppImage"));

        StringBuilder releaseBody = new();
        releaseBody.AppendLine(
                $"## {Format.Url($"Ryujinx{(!isCanary ? " Stable" : string.Empty)} {latest.Name}", latest.HtmlUrl)}")
            .AppendLine(DiscordHelper.Zws).AppendLine("### Downloads");
        var downloads = 0;
        
        applyArtifact(windowsX64, "Windows x64");
        applyArtifacts((linuxX64, linuxX64AppImage), "Linux x64");
        applyArtifact(macOs, "macOS Universal");
        applyArtifacts((linuxArm64, linuxArm64AppImage), "Linux ARM64");
        applyArtifact(windowsArm64, "Windows ARM64");

        return Ok(CreateReplyBuilder()
            .WithEmbed(embed =>
            {
                embed.WithAuthor(latest.Author.Login, latest.Author.AvatarUrl, latest.HtmlUrl);
                embed.WithDescription($"{releaseBody}\n{downloads} total downloads");
                embed.WithTimestamp(latest.CreatedAt);
            }));
        
        void applyArtifact(ReleaseAsset asset, string friendlyName)
        {
            if (asset is null) 
                return;

            releaseBody.AppendLine($"{Format.Url(friendlyName, asset.BrowserDownloadUrl)}");
            downloads += asset.DownloadCount;
        }
        
        void applyArtifacts(
            (ReleaseAsset Normal, ReleaseAsset AppImage) asset, 
            string friendlyName)
        {
            if (asset.Normal != null)
            {
                releaseBody.Append($"{Format.Url(friendlyName, asset.Normal.BrowserDownloadUrl)} ");
                downloads += asset.Normal.DownloadCount;
            }

            if (asset.AppImage != null)
            {
                releaseBody.AppendLine($"({Format.Url("AppImage", asset.AppImage.BrowserDownloadUrl)})");
                downloads += asset.AppImage.DownloadCount;
            }
            else if (asset.Normal != null)
                releaseBody.AppendLine();
        }
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
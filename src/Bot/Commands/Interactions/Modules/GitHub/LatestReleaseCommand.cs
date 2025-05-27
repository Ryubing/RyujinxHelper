using Discord.Interactions;
using Octokit;

namespace RyuBot.Commands.Interactions.Modules;

public partial class GitHubModule
{
    [SlashCommand("latest", "Show the download URLs for the latest release of Ryujinx.")]
    [RequireNotPiratePrecondition]
    public async Task<RuntimeResult> LatestReleaseAsync(
        [Summary("release_channel",
            "The release channel to look for the latest version from. Only has more options in Ryubing.")]
        [Autocomplete<ReleaseChannelAutocompleter>]
        string releaseChannel = "default")
    {
        if (releaseChannel is "default") 
            releaseChannel = (Context.Channel as INestedChannel)?.CategoryId is 1372043401540665376 
                ? GitHubService.KenjinxReleases // Kenji-NX category default
                : "Stable"; // Everywhere else default
        
        var latest = (Context.Channel as INestedChannel)?.CategoryId switch
        {
            1372043401540665376 => 
                releaseChannel.EqualsIgnoreCase(GitHubService.KenjinxReleases) 
                    ? await GitHub.GetLatestKenjinxReleaseAsync()
                    : releaseChannel.EqualsIgnoreCase(GitHubService.KenjinxAndroidReleases) 
                        ? await GitHub.GetLatestKenjinxAndroidReleaseAsync()
                        : null,
            _ => releaseChannel.EqualsIgnoreCase("Canary")
                    ? await GitHub.GetLatestCanaryAsync()
                    : releaseChannel.EqualsIgnoreCase("Stable")
                        ? await GitHub.GetLatestStableAsync()
                        : null
        };
        
        if (latest is null)
            return BadRequest(
                $"Unknown release channel {releaseChannel}. Please wait for the autocomplete suggestions to fill in if you aren't sure what to put!");

        var assets = latest.Assets.Where(x =>
            !x.Name.ContainsIgnoreCase("nogui") && !x.Name.ContainsIgnoreCase("headless")
        ).ToArray();

        var windowsX64 = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("win_x64"));
        var windowsArm64 = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("win_arm64"));
        var linuxX64 = assets.FirstOrDefault(x =>
            x.Name.ContainsIgnoreCase("linux_x64") && !x.Name.EndsWithIgnoreCase(".AppImage"));
        var linuxX64AppImage =
            assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("x64") && x.Name.EndsWithIgnoreCase(".AppImage"));
        var macOs = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("macos_universal"));
        var linuxArm64 = assets.FirstOrDefault(x =>
            x.Name.ContainsIgnoreCase("linux_arm64") && !x.Name.EndsWithIgnoreCase(".AppImage"));
        var linuxArm64AppImage = assets.FirstOrDefault(x =>
            x.Name.ContainsIgnoreCase("arm64") && x.Name.EndsWithIgnoreCase(".AppImage"));
        
        var androidApk = assets.FirstOrDefault(x => x.Name.EndsWithIgnoreCase(".apk"));

        StringBuilder releaseBody = new();
        releaseBody.AppendLine(
                $"## {Format.Url($"{(releaseChannel.ContainsIgnoreCase("kenji") ? "Kenji-NX" : "Ryujinx")} {latest.Name}".Trim(), latest.HtmlUrl)}")
            .AppendLine(DiscordHelper.Zws).AppendLine("### Downloads");
        var downloads = 0;
        
        applyArtifact(windowsX64, "Windows x64");
        applyArtifacts((linuxX64, linuxX64AppImage), "Linux x64");
        applyArtifact(macOs, "macOS Universal");
        applyArtifacts((linuxArm64, linuxArm64AppImage), "Linux ARM64");
        applyArtifact(windowsArm64, "Windows ARM64");
        applyArtifact(androidApk, "Android APK");
        
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
using Discord.Interactions;
using Octokit;
using Release = ForgejoApiClient.Api.Release;

namespace RyuBot.Commands.Interactions.Modules;

public partial class GitHubModule
{
    [SlashCommand("latest", "Show the download URLs for the latest release of Ryujinx.")]
    [RequireNotPiratePrecondition]
    public Task<RuntimeResult> LatestReleaseAsync(
        [Summary("release_channel",
            "The release channel to look for the latest version from.")]
        [Autocomplete<ReleaseChannelAutocompleter>]
        string releaseChannel = "default")
    {
        if (releaseChannel is "default") 
            releaseChannel = (Context.Channel as INestedChannel)?.CategoryId is 1372043401540665376 
                ? GitHubService.KenjinxReleases // Kenji-NX category default
                : "Stable"; // Everywhere else default
        
        return (Context.Channel as INestedChannel)?.CategoryId switch
        {
            1372043401540665376 => LatestKenjinxReleaseAsync(releaseChannel),
            _ => LatestRyubingReleaseAsync(releaseChannel)
        };
    }
    
    private async Task<RuntimeResult> LatestRyubingReleaseAsync(string rc)
    {
        var latest = rc.EqualsIgnoreCase("Canary")
            ? await ForgejoService.GetLatestCanaryAsync()
            : rc.EqualsIgnoreCase("Stable")
                ? await ForgejoService.GetLatestStableAsync()
                : null;
        
        if (latest is null)
            return BadRequest(
                $"Unknown release channel {rc}. Please wait for the autocomplete suggestions to fill in if you aren't sure what to put!");

        var windowsX64 = latest.assets?
            .FirstOrDefault(x => x.name.ContainsIgnoreCase("win_x64"));
        var windowsArm64 = latest.assets?
            .FirstOrDefault(x => x.name.ContainsIgnoreCase("win_arm64"));
        var linuxX64 = latest.assets?
            .FirstOrDefault(x => x.name.ContainsIgnoreCase("linux_x64"));
        var linuxX64AppImage = latest.assets?
            .FirstOrDefault(x => x.name.EndsWithIgnoreCase("x64.AppImage"));
        var macOs = latest.assets?
            .FirstOrDefault(x => x.name.ContainsIgnoreCase("macos_universal"));
        var linuxArm64 = latest.assets?
            .FirstOrDefault(x => x.name.ContainsIgnoreCase("linux_arm64"));
        var linuxArm64AppImage = latest.assets?
            .FirstOrDefault(x => x.name.EndsWithIgnoreCase("arm64.AppImage"));

        StringBuilder releaseBody = new();
        releaseBody.Append($"Total downloads: {latest.assets?.Sum(x => x.download_count ?? 0) ?? 0}").AppendLine(DiscordHelper.Zws).AppendLine("### Links");
        
        applyArtifact(windowsX64, "Windows x64");
        applyArtifacts((linuxX64, linuxX64AppImage), "Linux x64");
        applyArtifact(macOs, "macOS Universal");
        applyArtifacts((linuxArm64, linuxArm64AppImage), "Linux ARM64");
        applyArtifact(windowsArm64, "Windows ARM64");
        
        return Ok(CreateReplyBuilder()
            .WithEmbed(embed =>
            {
                embed.WithTitle($"Ryujinx {latest.name}".Trim()).WithUrl(latest.html_url);
                embed.WithAuthor(latest.author!.login_name, latest.author.avatar_url);
                embed.WithDescription(releaseBody);
                embed.WithTimestamp(latest.created_at ?? DateTimeOffset.Now);
            }));

        void applyArtifact(ForgejoApiClient.Api.Attachment asset, string friendlyName)
        {
            if (asset is null)
                return;

            releaseBody.AppendLine($"{Format.Url(friendlyName, asset.browser_download_url)}");
        }

        void applyArtifacts(
            (ForgejoApiClient.Api.Attachment Normal, ForgejoApiClient.Api.Attachment AppImage) asset,
            string friendlyName)
        {
            if (asset.Normal != null)
            {
                releaseBody.Append($"{Format.Url(friendlyName, asset.Normal.browser_download_url)} ");
            }

            if (asset.AppImage != null)
            {
                releaseBody.AppendLine($"({Format.Url("AppImage", asset.AppImage.browser_download_url)})");
            }
            else if (asset.Normal != null)
                releaseBody.AppendLine();
        }
    }

    private async Task<RuntimeResult> LatestKenjinxReleaseAsync(string rc)
    {
        var latest = rc.EqualsIgnoreCase(GitHubService.KenjinxReleases)
            ? await GitHub.GetLatestKenjinxReleaseAsync()
            : rc.EqualsIgnoreCase(GitHubService.KenjinxAndroidReleases)
                ? await GitHub.GetLatestKenjinxAndroidReleaseAsync()
                : null;
        
        if (latest is null)
            return BadRequest(
                $"Unknown release channel {rc}. Please wait for the autocomplete suggestions to fill in if you aren't sure what to put!");

        var assets = latest.Assets.Where(x =>
            !x.Name.ContainsIgnoreCase("nogui") && !x.Name.ContainsIgnoreCase("headless")
        ).ToArray();

        var windowsX64 = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("win_x64"));
        var windowsArm64 = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("win_arm64"));
        var linuxX64 = assets.FirstOrDefault(x =>
            x.Name.ContainsIgnoreCase("linux_x64") && !x.Name.EndsWithIgnoreCase(".AppImage"));
        var linuxX64AppImage =
            assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("x64") && x.Name.EndsWithIgnoreCase(".AppImage"));
        var macOsUniversal = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("macos_universal"));
        var macOsArm64 = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("macos_arm64"));
        var linuxArm64 = assets.FirstOrDefault(x =>
            x.Name.ContainsIgnoreCase("linux_arm64") && !x.Name.EndsWithIgnoreCase(".AppImage"));
        var linuxArm64AppImage = assets.FirstOrDefault(x =>
            x.Name.ContainsIgnoreCase("arm64") && x.Name.EndsWithIgnoreCase(".AppImage"));
        
        var androidApk = assets.FirstOrDefault(x => x.Name.EndsWithIgnoreCase(".apk"));

        StringBuilder releaseBody = new();
        releaseBody.AppendLine(DiscordHelper.Zws).AppendLine("### Downloads");
        var downloads = 0;
        
        applyArtifact(windowsX64, "Windows x64");
        applyArtifacts((linuxX64, linuxX64AppImage), "Linux x64");
        applyArtifact(macOsUniversal, "macOS Universal");
        applyArtifact(macOsArm64, "macOS (Apple Silicon only)");
        applyArtifacts((linuxArm64, linuxArm64AppImage), "Linux ARM64");
        applyArtifact(windowsArm64, "Windows ARM64");
        applyArtifact(androidApk, "Android APK");
        
        return Ok(CreateReplyBuilder()
            .WithEmbed(embed =>
            {
                embed.WithTitle($"Kenji-NX {latest.Name}".Trim()).WithUrl(latest.HtmlUrl);
                embed.WithAuthor(latest.Author.Login, latest.Author.AvatarUrl, latest.Author.HtmlUrl);
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
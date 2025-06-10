using Discord.Interactions;
using Octokit;

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
            ? await GitLab.GetLatestCanaryAsync()
            : rc.EqualsIgnoreCase("Stable")
                ? await GitLab.GetLatestStableAsync()
                : null;
        
        if (latest is null)
            return BadRequest(
                $"Unknown release channel {rc}. Please wait for the autocomplete suggestions to fill in if you aren't sure what to put!");

        var windowsX64 = latest.Assets.Links
            .FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("win_x64"));
        var windowsArm64 = latest.Assets.Links
            .FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("win_arm64"));
        var linuxX64 = latest.Assets.Links
            .FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("linux_x64"));
        var linuxX64AppImage = latest.Assets.Links
            .FirstOrDefault(x => x.AssetName.EndsWithIgnoreCase("x64.AppImage"));
        var macOs = latest.Assets.Links
            .FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("macos_universal"));
        var linuxArm64 = latest.Assets.Links
            .FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("linux_arm64"));
        var linuxArm64AppImage = latest.Assets.Links
            .FirstOrDefault(x => x.AssetName.EndsWithIgnoreCase("arm64.AppImage"));

        StringBuilder releaseBody = new();
        releaseBody.AppendLine(
                $"## {Format.Url($"Ryujinx {latest.Name}".Trim(), latest.Links.Self)}")
            .AppendLine(DiscordHelper.Zws).AppendLine("### Downloads");
        
        applyArtifact(windowsX64, "Windows x64");
        applyArtifacts((linuxX64, linuxX64AppImage), "Linux x64");
        applyArtifact(macOs, "macOS Universal");
        applyArtifacts((linuxArm64, linuxArm64AppImage), "Linux ARM64");
        applyArtifact(windowsArm64, "Windows ARM64");
        
        return Ok(CreateReplyBuilder()
            .WithEmbed(embed =>
            {
                embed.WithAuthor(latest.Author.Name, latest.Author.AvatarUrl, latest.Links.Self);
                embed.WithDescription(releaseBody);
                embed.WithTimestamp(latest.CreatedAt);
            }));

        void applyArtifact(GitLabReleaseJsonResponse.AssetLink asset, string friendlyName)
        {
            if (asset is null)
                return;

            releaseBody.AppendLine($"{Format.Url(friendlyName, asset.Url)}");
        }

        void applyArtifacts(
            (GitLabReleaseJsonResponse.AssetLink Normal, GitLabReleaseJsonResponse.AssetLink AppImage) asset,
            string friendlyName)
        {
            if (asset.Normal != null)
            {
                releaseBody.Append($"{Format.Url(friendlyName, asset.Normal.Url)} ");
            }

            if (asset.AppImage != null)
            {
                releaseBody.AppendLine($"({Format.Url("AppImage", asset.AppImage.Url)})");
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
        var macOs = assets.FirstOrDefault(x => x.Name.ContainsIgnoreCase("macos_universal"));
        var linuxArm64 = assets.FirstOrDefault(x =>
            x.Name.ContainsIgnoreCase("linux_arm64") && !x.Name.EndsWithIgnoreCase(".AppImage"));
        var linuxArm64AppImage = assets.FirstOrDefault(x =>
            x.Name.ContainsIgnoreCase("arm64") && x.Name.EndsWithIgnoreCase(".AppImage"));
        
        var androidApk = assets.FirstOrDefault(x => x.Name.EndsWithIgnoreCase(".apk"));

        StringBuilder releaseBody = new();
        releaseBody.AppendLine(
                $"## {Format.Url($"Kenji-NX {latest.Name}".Trim(), latest.HtmlUrl)}")
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
using Discord.Interactions;

namespace RyuBot.Commands.Interactions.Modules;

public partial class GitLabModule
{
    [SlashCommand("latest", "Show the download URLs for the latest release of Ryujinx.")]
    [RequireNotPiratePrecondition]
    public async Task<RuntimeResult> LatestReleaseAsync(
        [Summary("release_channel",
            "The release channel to look for the latest version from.")]
        [Autocomplete<GitLabReleaseChannelAutocompleter>]
        string releaseChannel = "Stable")
    {
        var latest = releaseChannel.EqualsIgnoreCase("Canary")
            ? await GitLab.GetLatestCanaryAsync()
            : releaseChannel.EqualsIgnoreCase("Stable")
                ? await GitLab.GetLatestStableAsync()
                : null;
        
        if (latest is null)
            return BadRequest(
                $"Unknown release channel {releaseChannel}. Please wait for the autocomplete suggestions to fill in if you aren't sure what to put!");

        var assets = latest.Assets.Links.Where(x =>
            !x.AssetName.ContainsIgnoreCase("nogui") && !x.AssetName.ContainsIgnoreCase("headless")
        ).ToArray();

        var windowsX64 = assets.FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("win_x64"));
        var windowsArm64 = assets.FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("win_arm64"));
        var linuxX64 = assets.FirstOrDefault(x =>
            x.AssetName.ContainsIgnoreCase("linux_x64") && !x.AssetName.EndsWithIgnoreCase(".AppImage"));
        var linuxX64AppImage =
            assets.FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("x64") && x.AssetName.EndsWithIgnoreCase(".AppImage"));
        var macOs = assets.FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("macos_universal"));
        var linuxArm64 = assets.FirstOrDefault(x =>
            x.AssetName.ContainsIgnoreCase("linux_arm64") && !x.AssetName.EndsWithIgnoreCase(".AppImage"));
        var linuxArm64AppImage = assets.FirstOrDefault(x =>
            x.AssetName.ContainsIgnoreCase("arm64") && x.AssetName.EndsWithIgnoreCase(".AppImage"));
        
        var androidApk = assets.FirstOrDefault(x => x.AssetName.EndsWithIgnoreCase(".apk"));

        StringBuilder releaseBody = new();
        releaseBody.AppendLine(
                $"## {Format.Url($"Ryujinx {latest.Name}".Trim(), latest.Links.Self)}")
            .AppendLine(DiscordHelper.Zws).AppendLine("### Downloads");
        
        applyArtifact(windowsX64, "Windows x64");
        applyArtifacts((linuxX64, linuxX64AppImage), "Linux x64");
        applyArtifact(macOs, "macOS Universal");
        applyArtifacts((linuxArm64, linuxArm64AppImage), "Linux ARM64");
        applyArtifact(windowsArm64, "Windows ARM64");
        applyArtifact(androidApk, "Android APK");
        
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
}
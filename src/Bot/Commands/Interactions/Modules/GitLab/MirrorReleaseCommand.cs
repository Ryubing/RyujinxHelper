using Discord.Interactions;
using NGitLab.Models;
using Octokit;

namespace RyuBot.Commands.Interactions.Modules;

public partial class GitLabModule
{
    [SlashCommand("mirror-release", "Mirrors a release from GitHub onto the GitLab.")]
    [RequireBotOwnerPrecondition]
    public async Task<RuntimeResult> MirrorReleaseAsync(
        [Autocomplete<ReleaseChannelAutocompleter>]
        string releaseChannel,
        string version,
        string project = "ryubing/ryujinx")
    {
        await DeferAsync(true);

        try
        {
            var githubRelease = releaseChannel.EqualsIgnoreCase("Canary")
                ? await GitHub.GetCanaryReleaseAsync(version)
                : await GitHub.GetStableReleaseAsync(version);

            var assets = githubRelease.Assets.Where(x =>
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

            List<ReleaseLink> gitlabAssetLinks = [];

            applyArtifacts(windowsX64, windowsArm64, linuxX64, linuxX64AppImage, macOs, linuxArm64, linuxArm64AppImage);

            var gitlabRelease = await GitLab.Client.GetReleases(project).CreateAsync(new ReleaseCreate
            {
                TagName = githubRelease.TagName,
                Description = githubRelease.Body,
                Name = githubRelease.Name,
                ReleasedAt = githubRelease.CreatedAt.DateTime,
                Assets = new ReleaseAssetsInfo
                {
                    Count = gitlabAssetLinks.Count,
                    Links = gitlabAssetLinks.ToArray()
                }
            });

            return Ok($"Release mirrored successfully. View it {Format.Url("here", $"{Config.GitLabAuth.InstanceUrl}/{project}/-/releases/{gitlabRelease.TagName}")}.");
            
            void applyArtifacts(params ReleaseAsset[] rs)
            {
                foreach (var asset in rs)
                {
                    if (asset is null) continue;
            
                    gitlabAssetLinks.Add(new ReleaseLink
                    {
                        External = true,
                        LinkType = ReleaseLinkType.Package, 
                        Name = asset.Name,
                        Url = asset.BrowserDownloadUrl
                    });
                }
            }
        }
        catch (Exception e)
        {
            return BadRequest(Format.Code(e.ToString(), string.Empty));
        }
    }
}
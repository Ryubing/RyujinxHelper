using Discord.Interactions;
using NGitLab.Models;

namespace RyuBot.Commands.Interactions.Modules;

public partial class GitLabModule
{
    [SlashCommand("mirror-release", "Mirrors a release from GitHub onto the GitLab.")]
    [RequireProjectMaintainerPrecondition]
    public async Task<RuntimeResult> MirrorReleaseAsync(
        [Autocomplete<AuthorizedReleaseChannelAutocompleter>]
        [Summary("release_channel", "The source of the release.")]
        string releaseChannel,
        [Autocomplete<TargetProjectAutocompleter>]
        [Summary(description: "The target GitLab project of the mirrored release.")]
        string project,
        [Summary(description: "The raw version string of the tag of the release.")]
        string version)
    {
        await DeferAsync(true);

        try
        {
            if (!AuthorizedReleaseChannelAutocompleter.CanMirror(Context.User.Id, releaseChannel))
                return BadRequest("You are not able to mirror releases from this GitHub repository.");
            
            if (!TargetProjectAutocompleter.CanTarget(Context.User.Id, project))
                return BadRequest("You are not able to perform operations on this GitLab project.");

            var githubRelease = Context.User.Id switch
            {
                RequireProjectMaintainerPreconditionAttribute.GreemDev =>
                    releaseChannel.EqualsIgnoreCase("Canary")
                        ? await GitHub.GetCanaryReleaseAsync(version)
                        : releaseChannel.EqualsIgnoreCase("Stable")
                            ? await GitHub.GetStableReleaseAsync(version)
                            : null,
                RequireProjectMaintainerPreconditionAttribute.Keaton => 
                    releaseChannel.EqualsIgnoreCase(GitHubService.KenjinxReleases)
                        ? await GitHub.GetKenjinxReleaseAsync(version)
                        : releaseChannel.EqualsIgnoreCase(GitHubService.KenjinxAndroidReleases)
                            ? await GitHub.GetKenjinxAndroidReleaseAsync(version)
                            : null,
                _ => null
            };

            if (githubRelease is null)
                return BadRequest("How did you reach this part of the code, there's like 2 checks before this?");

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
            
            var androidApk = assets.FirstOrDefault(x => x.Name.EndsWithIgnoreCase(".apk"));

            
            var gitlabAssetLinks = Collections.NewArray(
                    windowsX64, windowsArm64, 
                    linuxX64, linuxX64AppImage, 
                    macOs, 
                    linuxArm64, linuxArm64AppImage, 
                    androidApk)
                .Where(x => x is not null)
                .Select(x => new ReleaseLink
                {
                    External = true,
                    LinkType = ReleaseLinkType.Package,
                    Name = x.Name,
                    Url = x.BrowserDownloadUrl
                })
                .ToArray();

            var gitlabRelease = await GitLab.Client.GetReleases(project).CreateAsync(new ReleaseCreate
            {
                TagName = githubRelease.TagName,
                Description = githubRelease.Body,
                Name = githubRelease.Name,
                ReleasedAt = githubRelease.CreatedAt.DateTime,
                Assets = new ReleaseAssetsInfo
                {
                    Count = gitlabAssetLinks.Length,
                    Links = gitlabAssetLinks
                }
            });

            return Ok($"Release mirrored successfully. View it {Format.Url("here", $"{Config.GitLabAuth.InstanceUrl}/{project}/-/releases/{gitlabRelease.TagName}")}.");
        }
        catch (Exception e)
        {
            return BadRequest(Format.Code(e.ToString(), string.Empty));
        }
    }
}
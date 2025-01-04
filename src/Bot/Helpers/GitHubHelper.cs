using Octokit;

namespace RyuBot.Helpers;

public static class GitHubHelper
{
    public const string MainRepoOwner = "Ryubing";
    public const string MirrorRepoOwner = "ryujinx-mirror";
    public const string MainRepoName = "Ryujinx";
    
    public static (string Owner, string Name) GetRepo(IInteractionContext ctx) => 
        ctx.Guild?.Id switch
        {
            1291765437100720243 => (MirrorRepoOwner, MainRepoName.ToLower()),
            _ => (MainRepoOwner, MainRepoName)
        };

    public static string Format(this (string Owner, string Name) repoTuple) => $"{repoTuple.Owner}/{repoTuple.Name}";

    public static string FormatLabels(this PullRequest pullRequest)
        => pullRequest.Labels.Count > 0 ? pullRequest.Labels.Select(FormatLabelName).JoinToString(", ") : "None";
    
    public static string FormatLabels(this Issue issue)
        => issue.Labels.Count > 0 ? issue.Labels.Select(FormatLabelName).JoinToString(", ") : "None";

    public static string FormatLabelName(this Label lbl) => FormatLabelName(lbl.Name);
    
    public static string FormatLabelName(string labelName) => labelName.ToLower() switch
    {
        "audio" => "Audio",
        "bug" => "Bug",
        "cpu" => "CPU",
        "gpu" => "GPU",
        "gui" => "GUI",
        "help wanted" => "Help Wanted",
        "horizon" => "Horizon",
        "infra" => "Project Infra",
        "invalid" => "Invalid",
        "kernel" => "Kernel",
        "ldn" => "LDN",
        "linux" => "Linux",
        "macos" => "macOS",
        "question" => "Question",
        "windows" => "Windows",
        "wontfix" => "Won't Fix",
        "documentation" => "Documentation",
        "duplicate" => "Duplicate",
        "enhancement" => "Enhancement",
        "good first issue" => "Good First Issue",
        "graphics-backend:opengl" => "Graphics: OpenGL",
        "graphics-backend:vulkan" => "Graphics: Vulkan",
        "not planned but open to a PR" => "Not planned, but open to a PR",
        "blocked on external progress" => "Blocked on External Progress",
        _ => labelName
    };

}
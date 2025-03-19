﻿using Octokit;

namespace RyuBot.Helpers;

public static class GitHubHelper
{
    public const string MainRepoOwner = "Ryubing";
    public const string MainRepoName = "Ryujinx";

    public static string Format(this (string Owner, string Name) repoTuple) => $"{repoTuple.Owner}/{repoTuple.Name}";

    public static string FormatLabels(this PullRequest pullRequest)
        => pullRequest.Labels.FormatCollection(FormatLabelName, separator: ", ");

    public static string FormatLabels(this Issue issue)
        => issue.Labels.FormatCollection(FormatLabelName, separator: ", ");

    public static string FormatLabelName(this Label lbl) => FormatLabelName(lbl.Name);
    
    public static string FormatLabelName(string labelName, bool markdown = true) => labelName.ToLower() switch
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
        
        // old compat list labels
        
        "ldn-works" => "LDN Works",
        "ldn-untested" => "LDN Untested",
        "ldn-broken" => "LDN Broken",
        "ldn-partial" => "Partial LDN",
        "nvdec" => "NVDEC",
        "services" => "NX Services",
        "services-horizon" => "Horizon OS Services",
        "slow" => "Runs Slow",
        "crash" => "Crashes",
        "deadlock" => markdown ? "[Deadlock](<https://wikipedia.org/wiki/Deadlock_(computer_science)>)" : "Deadlock",
        "regression" => markdown ? "[Regression](<https://wikipedia.org/wiki/Software_regression>)" : "Regression",
        "opengl" => "OpenGL",
        "opengl-backend-bug" => "OpenGL Backend Bug",
        "vulkan-backend-bug" => "Vulkan Backend Bug",
        "mac-bug" => "Mac-specific Bug(s)",
        "amd-vendor-bug" => "AMD GPU Bug",
        "intel-vendor-bug" => "Intel GPU Bug",
        "loader-allocator" => "Loader Allocator",
        "audout" => "AudOut",
        "32-bit" => "32-bit Game",
        "UE4" => "Unreal Engine 4",
        "homebrew" => "Homebrew Content",
        "online-broken" => "Online Broken",
        _ => labelName.Capitalize()
    };

}
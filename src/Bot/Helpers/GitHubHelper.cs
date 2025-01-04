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
}
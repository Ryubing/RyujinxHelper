using System.Text.RegularExpressions;

namespace RyuBot.Services;

public static class LogAnalysisPatterns
{
    public static readonly Regex StableVersion = StableVersionRegex();
    public static readonly Regex CanaryVersion = CanaryVersionRegex();
    public static readonly Regex OriginalProjectVersion = OriginalProjectVersionRegex();
    public static readonly Regex OriginalProjectLdnVersion = OriginalProjectLdnVersionRegex();
    public static readonly Regex PrVersion = PrVersionRegex();
    public static readonly Regex MirrorVersion = MirrorVersionRegex();
    
    [GeneratedRegex(@"^1\.2\.\d+$")]
    private static extern Regex StableVersionRegex();

    [GeneratedRegex(@"^Canary 1\.2\.\d+$")]
    private static extern Regex CanaryVersionRegex();
    
    [GeneratedRegex(@"^1\.(0|1)\.\d+$")]
    private static extern Regex OriginalProjectVersionRegex();
    
    [GeneratedRegex(@"^\d\.\d\.\d-ldn\d+\.\d+(?:\.\d+|$)")]
    private static extern Regex OriginalProjectLdnVersionRegex();
    
    [GeneratedRegex(@"^1\.2\.\d\+([a-f]|\d){7}$")]
    private static extern Regex PrVersionRegex();
    
    [GeneratedRegex(@"^r\.(\d|\w){7}$")]
    private static extern Regex MirrorVersionRegex();
}
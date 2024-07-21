using System.IO;

namespace Gommon;

/// <summary>
///     Extensions for any class in the System namespace, including sub-namespaces, such as System.Text.
/// </summary>
public static partial class Extensions
{
    public static bool ExistsInAny<T>(this T @this, params IEnumerable<T>[] collections) 
        => collections.Any(x => x.Contains(@this));

    public static string CalculateUptime(this Process process)
        => (DateTime.Now - process.StartTime).Humanize(3);

    public static Task<IUserMessage> SendFileToAsync(this MemoryStream stream,
        ITextChannel channel, string filename, string text = null, bool isTts = false, Embed embed = null,
        RequestOptions options = null,
        bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference reference = null)
        => channel.SendFileAsync(stream, filename, text, isTts, embed, options, isSpoiler, allowedMentions,
            reference);

    public static string FormatBoldString(this DateTime dt)
        => dt.FormatPrettyString().Split(" ").Apply(arr =>
        {
            arr[1] = Format.Bold(arr[1]);
            arr[2] = $"{Format.Bold(arr[2].TrimEnd(','))},";
            arr[4] = Format.Bold(arr[4]);
        }).JoinToString(" ");

    public static string FormatBoldString(this DateTimeOffset dt) 
        => dt.DateTime.FormatBoldString();


    public static string Truncate(this string value, int limit, Func<int, string> truncationString)
    {
        var content = value.Take(limit).JoinToString(string.Empty);
        var truncatedCharacters = value.Length - content.Length;
        if (truncatedCharacters > 0)
            content += truncationString(truncatedCharacters);

        return content;
    }

    public static void SentryCapture(this Exception e, Action<Scope> configureScope = null)
    {
        if (e is TaskCanceledException or OperationCanceledException) return;
        
        if (configureScope != null)
            SentrySdk.CaptureException(e, configureScope);
        else SentrySdk.CaptureException(e);

    }
    
    #nullable enable
    
    public static bool TryParse<T>(this string? s, [MaybeNullWhen(false)] out T result, IFormatProvider formatProvider = null) where T : IParsable<T>
        => T.TryParse(s, formatProvider, out result);
    
    public static T Parse<T>(this string? s, IFormatProvider formatProvider = null) where T : IParsable<T>
        => T.Parse(s, formatProvider);
    
    public static bool TryParse<T>(this ReadOnlySpan<char> s, [MaybeNullWhen(false)] out T result, IFormatProvider formatProvider = null) where T : ISpanParsable<T>
        => T.TryParse(s, formatProvider, out result);
    
    public static T Parse<T>(this ReadOnlySpan<char> s, IFormatProvider formatProvider = null) where T : ISpanParsable<T>
        => T.Parse(s, formatProvider);
}
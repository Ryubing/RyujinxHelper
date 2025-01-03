namespace RyuBot.Helpers;

public static class HttpHelper
{
    /// <summary>
    ///     POSTs the specified string <paramref name="content"/> to <paramref name="url"/> with the <see cref="HttpClient"/> from <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">The <see cref="IServiceProvider"/> containing the <see cref="HttpClient"/>.</param>
    /// <param name="url">The URL to POST to.</param>
    /// <param name="content">The content to POST.</param>
    /// <returns>The resulting <see cref="HttpResponseMessage"/>.</returns>
    public static Task<HttpResponseMessage> PostStringAsync(IServiceProvider provider, string url, string content) 
        => provider.Get<HttpClient>().PostAsync(url, new StringContent(content, Encoding.UTF8, "text/plain"));
}
#nullable enable
using System.Net;

namespace RyuBot.Helpers;

public interface IHttpClientProxy
{
    /// <summary>
    /// </summary>
    /// <param name="actualCaller"></param>
    /// <param name="request"></param>
    /// <param name="option"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <remarks>Do not call. Use an overload.</remarks>
    protected Task<HttpResponseMessage> SendAsync(string actualCaller, HttpRequestMessage request,
        HttpCompletionOption? option = null, CancellationToken? token = null);

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption? option = null,
        CancellationToken? token = null)
        => SendAsync(nameof(SendAsync), request, option, token);

    public ForgejoPaginatedEndpoint<T> Paginate<T>(
        Func<ForgejoPaginatedEndpoint<T>.BuilderApi, ForgejoPaginatedEndpoint<T>.BuilderApi> builder)
        => ForgejoPaginatedEndpoint<T>.Builder(this).Into(builder);

    #region Convenience overloads for SendAsync

    public Task<HttpResponseMessage> GetAsync(
        [StringSyntax(StringSyntaxAttribute.Uri)]
        string requestUri,
        HttpCompletionOption? option = null, CancellationToken? token = null
    ) => GetAsync(CreateUri(requestUri)!, option, token);

    public Task<HttpResponseMessage> PostAsync(
        [StringSyntax(StringSyntaxAttribute.Uri)]
        string requestUri,
        HttpContent? content = null,
        HttpCompletionOption? option = null, CancellationToken? token = null
    ) => PostAsync(CreateUri(requestUri)!, content, option, token);

    public Task<HttpResponseMessage> PutAsync(
        [StringSyntax(StringSyntaxAttribute.Uri)]
        string requestUri,
        HttpContent? content = null,
        HttpCompletionOption? option = null, CancellationToken? token = null
    ) => PutAsync(CreateUri(requestUri)!, content, option, token);

    public Task<HttpResponseMessage> PatchAsync(
        [StringSyntax(StringSyntaxAttribute.Uri)]
        string requestUri,
        HttpContent? content = null,
        HttpCompletionOption? option = null, CancellationToken? token = null
    ) => PatchAsync(CreateUri(requestUri)!, content, option, token);

    public Task<HttpResponseMessage> DeleteAsync(
        [StringSyntax(StringSyntaxAttribute.Uri)]
        string requestUri,
        HttpContent? content = null,
        HttpCompletionOption? option = null, CancellationToken? token = null
    ) => DeleteAsync(CreateUri(requestUri)!, content, option, token);

    #region Uri overloads

    public Task<HttpResponseMessage> GetAsync(
        Uri requestUri,
        HttpCompletionOption? option = null, CancellationToken? token = null
    ) => SendAsync(nameof(GetAsync), CreateRequestMessage(HttpMethod.Get, requestUri), option, token);

    public Task<HttpResponseMessage> PostAsync(
        Uri requestUri,
        HttpContent? content = null,
        HttpCompletionOption? option = null, CancellationToken? token = null
    ) => SendAsync(nameof(PostAsync), CreateRequestMessageWithContent(HttpMethod.Post, requestUri, content), option,
        token);

    public Task<HttpResponseMessage> PutAsync(
        Uri requestUri,
        HttpContent? content = null,
        HttpCompletionOption? option = null, CancellationToken? token = null
    ) => SendAsync(nameof(PutAsync), CreateRequestMessageWithContent(HttpMethod.Put, requestUri, content), option,
        token);

    public Task<HttpResponseMessage> PatchAsync(
        Uri requestUri,
        HttpContent? content = null,
        HttpCompletionOption? option = null, CancellationToken? token = null
    ) => SendAsync(nameof(PatchAsync), CreateRequestMessageWithContent(HttpMethod.Patch, requestUri, content), option,
        token);

    public Task<HttpResponseMessage> DeleteAsync(
        Uri requestUri,
        HttpContent? content = null,
        HttpCompletionOption? option = null, CancellationToken? token = null
    ) => SendAsync(nameof(DeleteAsync), CreateRequestMessageWithContent(HttpMethod.Delete, requestUri, content), option,
        token);

    #endregion

    #endregion

    #region Overload Helpers

    private static HttpRequestMessage CreateRequestMessage(HttpMethod method, Uri? uri)
        => new(method, uri)
            { Version = HttpVersion.Version11, VersionPolicy = HttpVersionPolicy.RequestVersionOrLower };

    private static HttpRequestMessage CreateRequestMessageWithContent(HttpMethod method, Uri? uri,
        HttpContent? requestContent)
    {
        var req = CreateRequestMessage(method, uri);
        req.Content = requestContent;
        return req;
    }

    private static Uri? CreateUri(string? uri) =>
        string.IsNullOrEmpty(uri) ? null : new Uri(uri, UriKind.RelativeOrAbsolute);

    #endregion

    public class Default : IHttpClientProxy, IDisposable
    {
        private readonly HttpClient _http;

        public Default(HttpClient backingClient)
        {
            _http = backingClient;
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "RedundantAssignment",
            Justification =
                "ReSharper cannot comprehend the idea of checking all combinations of 2 objects potentially being null.")]
        public async Task<HttpResponseMessage> SendAsync(string actualCaller, HttpRequestMessage request,
            HttpCompletionOption? option = null, CancellationToken? token = null)
        {
            HttpResponseMessage response;

            var sw = Stopwatch.StartNew();

            if (option is null && token is not null)
                response = await _http.SendAsync(request, token.Value);
            if (option is not null && token is null)
                response = await _http.SendAsync(request, option.Value);
            if (option is not null && token is not null)
                response = await _http.SendAsync(request, option.Value, token.Value);
            else
                response = await _http.SendAsync(request);

            sw.Stop();

            Info(LogSource.Bot,
                $"{
                    request.Method.Method
                } {
                    request.RequestUri!.ToString()
                } -> {
                    (int)response.StatusCode
                } in {
                    sw.Elapsed.TotalMilliseconds
                }ms",
                new InvocationInfo(actualCaller)
            );

            return response;
        }

        public void Dispose() => _http.Dispose();
    }
}
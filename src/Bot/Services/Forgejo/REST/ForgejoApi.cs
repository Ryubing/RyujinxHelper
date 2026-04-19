using System.Net.Http.Headers;

namespace RyuBot.Services.Forgejo;

public static class ForgejoApi
{
    public static IHttpClientProxy CreateHttpClient(string host, string accessToken, TimeSpan? timeout = null)
        => new IHttpClientProxy.Default(new HttpClient
            {
                Timeout = timeout ?? TimeSpan.FromSeconds(100),
                BaseAddress = new Uri(host),
                DefaultRequestHeaders =
                {
                    UserAgent = { new ProductInfoHeaderValue("RyujinxHelper", "1.0.0") },
                    Authorization = AuthenticationHeaderValue.Parse($"Bearer {accessToken}")
                }
            }
        );
}
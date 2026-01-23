using System.Net.Http;

namespace NG.ControlCenter.WebSite.Services
{
    /// <summary>
    /// Configures HttpClient with resilience settings:
    /// - Timeout: 30 seconds per request
    /// - Connection pooling and keep-alive
    /// </summary>
    public static class HttpClientServiceExtensions
    {
        public static IHttpClientBuilder AddResilientHttpClient(this IServiceCollection services, string name)
        {
            return services.AddHttpClient(name)
                .ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Add("User-Agent", "NG.MicroERP.ControlCenter");
                    client.DefaultRequestHeaders.ConnectionClose = false;
                });
        }
    }
}

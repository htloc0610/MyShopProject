using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MyShop.Services.Auth
{
    /// <summary>
    /// HTTP message handler that automatically attaches Bearer token to requests.
    /// Used with HttpClientFactory - do NOT set InnerHandler as the factory manages the chain.
    /// </summary>
    public class AuthenticatedHttpClientHandler : DelegatingHandler
    {
        private readonly ISessionService _sessionService;

        public AuthenticatedHttpClientHandler(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            // Add Bearer token if user is logged in
            var accessToken = _sessionService.AccessToken;
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace assignment.Client.Services
{
    public class JwtAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly IAuthService _authService;

        public JwtAuthorizationMessageHandler(IAuthService authService)
        {
            _authService = authService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _authService.GetTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

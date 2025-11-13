using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace assignment.Client.Services
{
    public class JwtAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private const string TokenKey = "obituary_jwt";

        public JwtAuthenticationStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string?>("authStorage.get", TokenKey);

            if (string.IsNullOrWhiteSpace(token))
            {
                // anonymous user
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var identity = CreateClaimsIdentityFromJwt(token);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public async Task NotifyUserAuthenticated(string token)
        {
            await _jsRuntime.InvokeVoidAsync("authStorage.set", TokenKey, token);

            var identity = CreateClaimsIdentityFromJwt(token);
            var authState = new AuthenticationState(new ClaimsPrincipal(identity));
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        public void NotifyUserLoggedOut()
        {
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
        }

        private ClaimsIdentity CreateClaimsIdentityFromJwt(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                return new ClaimsIdentity(jwt.Claims, "jwt");
            }
            catch
            {
                return new ClaimsIdentity();
            }
        }
    }
}

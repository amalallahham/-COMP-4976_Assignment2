using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using assignment.Client.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace assignment.Client.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password);
        Task LogoutAsync();
        Task<string?> GetTokenAsync();
    }

    public class AuthService : IAuthService
    {
        private const string TokenKey = "obituary_jwt";
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(
            HttpClient httpClient,
            IJSRuntime jsRuntime,
            AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            // Matches POST api/auth/login with [FromBody] LoginRequest
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
            if (apiResponse == null || !apiResponse.Success || apiResponse.Data == null)
            {
                return false;
            }

            var authResponse = apiResponse.Data;

            // Save the token (rest of your logic stays the same)
            await _jsRuntime.InvokeVoidAsync("authStorage.set", TokenKey, authResponse.Token);

            if (_authStateProvider is JwtAuthenticationStateProvider jwtProvider)
            {
                await jwtProvider.NotifyUserAuthenticated(authResponse.Token);
            }

            return true;
        }


        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("authStorage.remove", TokenKey);

            if (_authStateProvider is JwtAuthenticationStateProvider jwtProvider)
            {
                jwtProvider.NotifyUserLoggedOut();
            }
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _jsRuntime.InvokeAsync<string?>("authStorage.get", TokenKey);
        }
    }
}

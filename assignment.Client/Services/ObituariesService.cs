using System.Net.Http.Json;
using System.Net.Http.Headers;
using assignment.Client.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace assignment.Client.Services
{
    public interface IObituariesService
    {
        Task<PagedResponse<ObituaryResponse>?> GetObituariesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null);

        Task<ObituaryResponse?> GetObituaryAsync(int id);
        Task<ApiResult> CreateObituaryAsync(CreateObituaryRequest request, IBrowserFile? photo);
        Task<ApiResult> UpdateObituaryAsync(int id, UpdateObituaryRequest request, IBrowserFile? photo);
        Task<bool> DeleteObituaryAsync(int id);
    }

    public class ObituariesService : IObituariesService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;

        public ObituariesService(HttpClient httpClient, IAuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        /// <summary>
        /// Reads JWT from storage via AuthService and attaches it to Authorization header.
        /// Used for protected endpoints (create, update, delete).
        /// </summary>
        private async Task AddAuthHeaderAsync()
        {
            // Clear any previous value to avoid stacking
            _httpClient.DefaultRequestHeaders.Authorization = null;

            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<PagedResponse<ObituaryResponse>?> GetObituariesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null)
        {
            var query = $"api/obituaries?pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(search))
            {
                query += $"&search={Uri.EscapeDataString(search)}";
            }

            var apiResponse =
                await _httpClient.GetFromJsonAsync<ApiResponse<PagedResponse<ObituaryResponse>>>(query);

            if (apiResponse == null || !apiResponse.Success)
            {
                return null;
            }

            return apiResponse.Data;
        }

        public async Task<ObituaryResponse?> GetObituaryAsync(int id)
        {
            var apiResponse =
                await _httpClient.GetFromJsonAsync<ApiResponse<ObituaryResponse>>($"api/obituaries/{id}");

            if (apiResponse == null || !apiResponse.Success)
            {
                return null;
            }

            return apiResponse.Data;
        }

        public async Task<ApiResult> CreateObituaryAsync(CreateObituaryRequest request, IBrowserFile? photo)
        {
            var result = new ApiResult();

            await AddAuthHeaderAsync(); // if you have this helper; if not, skip

            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(request.FullName), nameof(request.FullName));
            if (request.DOB.HasValue)
                content.Add(new StringContent(request.DOB.Value.ToString("o")), nameof(request.DOB));
            if (request.DOD.HasValue)
                content.Add(new StringContent(request.DOD.Value.ToString("o")), nameof(request.DOD));
            if (!string.IsNullOrWhiteSpace(request.Biography))
                content.Add(new StringContent(request.Biography), nameof(request.Biography));

            if (photo != null)
            {
                var stream = photo.OpenReadStream(10 * 1024 * 1024);
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(photo.ContentType);
                content.Add(fileContent, "Photo", photo.Name);
            }

            var response = await _httpClient.PostAsync("api/obituaries", content);

            if (response.IsSuccessStatusCode)
            {
                result.Success = true;
                return result;
            }

            // 400 with validation errors from model binding
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var problem = await response.Content
                    .ReadFromJsonAsync<HttpValidationProblemDetails>();

                if (problem?.Errors != null)
                {
                    foreach (var kvp in problem.Errors)
                    {
                        result.FieldErrors[kvp.Key] = kvp.Value;
                    }
                }

                result.ErrorMessage = "Validation failed. Please fix the highlighted fields.";
                return result;
            }

            // other errors
            result.Success = false;
            result.ErrorMessage = $"Error: {response.StatusCode}";
            return result;
        }

        public async Task<ApiResult> UpdateObituaryAsync(int id, UpdateObituaryRequest request, IBrowserFile? photo)
        {
            var result = new ApiResult();

            await AddAuthHeaderAsync(); // if you use JWT

            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(request.FullName), nameof(request.FullName));
            if (request.DOB.HasValue)
                content.Add(new StringContent(request.DOB.Value.ToString("o")), nameof(request.DOB));
            if (request.DOD.HasValue)
                content.Add(new StringContent(request.DOD.Value.ToString("o")), nameof(request.DOD));
            if (!string.IsNullOrWhiteSpace(request.Biography))
                content.Add(new StringContent(request.Biography), nameof(request.Biography));

            if (photo != null)
            {
                var stream = photo.OpenReadStream(10 * 1024 * 1024);
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(photo.ContentType);
                content.Add(fileContent, "Photo", photo.Name);
            }

            var response = await _httpClient.PutAsync($"api/obituaries/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                result.Success = true;
                return result;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
                if (problem?.Errors != null)
                {
                    foreach (var kvp in problem.Errors)
                    {
                        result.FieldErrors[kvp.Key] = kvp.Value;
                    }
                }

                result.Success = false;
                result.ErrorMessage = "Validation failed. Please fix the highlighted fields.";
                return result;
            }

            result.Success = false;
            result.ErrorMessage = $"Error: {response.StatusCode}";
            return result;
        }
        public async Task<bool> DeleteObituaryAsync(int id)
        {
            await AddAuthHeaderAsync(); // 🔐 requires authenticated user

            var response = await _httpClient.DeleteAsync($"api/obituaries/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}

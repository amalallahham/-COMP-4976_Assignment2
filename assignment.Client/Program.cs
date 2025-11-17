using assignment.Client;
using assignment.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Authorization for Blazor
builder.Services.AddAuthorizationCore();

// Auth + application services
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

// Register AuthService first with a plain HttpClient (no JWT handler needed for login)
builder.Services.AddScoped<IAuthService>(sp =>
{
    var httpClient = new HttpClient(new HttpClientHandler())
    {
        BaseAddress = new Uri("https://localhost:7184/")
    };
    var jsRuntime = sp.GetRequiredService<IJSRuntime>();
    var authStateProvider = sp.GetRequiredService<AuthenticationStateProvider>();
    return new AuthService(httpClient, jsRuntime, authStateProvider);
});

// HttpClient with JWT authorization handler (for other services like ObituariesService)
builder.Services.AddScoped<JwtAuthorizationMessageHandler>(sp =>
{
    var authService = sp.GetRequiredService<IAuthService>();
    var handler = new JwtAuthorizationMessageHandler(authService);
    handler.InnerHandler = new HttpClientHandler();
    return handler;
});

builder.Services.AddScoped<HttpClient>(sp =>
{
    var handler = sp.GetRequiredService<JwtAuthorizationMessageHandler>();
    return new HttpClient(handler)
    {
        BaseAddress = new Uri("https://localhost:7184/")
    };
});

builder.Services.AddScoped<IObituariesService, ObituariesService>();

await builder.Build().RunAsync();

using assignment.Client;
using assignment.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Authorization for Blazor
builder.Services.AddAuthorizationCore();

// Auth + application services
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();

// HttpClient with JWT authorization handler
builder.Services.AddScoped<JwtAuthorizationMessageHandler>();
builder.Services.AddScoped<HttpClient>(sp =>
{
    var authService = sp.GetRequiredService<IAuthService>();
    var handler = new JwtAuthorizationMessageHandler(authService)
    {
        InnerHandler = new HttpClientHandler()
    };
    return new HttpClient(handler)
    {
        BaseAddress = new Uri("http://localhost:5141/") // <-- your API URL
    };
});

builder.Services.AddScoped<IObituariesService, ObituariesService>();

await builder.Build().RunAsync();

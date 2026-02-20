using Blazored.LocalStorage;
using Bookify.Client;
using Bookify.Client.Auth;
using Bookify.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using static System.Net.WebRequestMethods;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ── HTTP Client (built-in to WASM host — no Microsoft.Extensions.Http needed) ─
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7031/" ?? "http://localhost:5138")
});

// ── Auth ──────────────────────────────────────────────────────────────────────
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, BookifyAuthStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IServiceApiService, ServiceApiService>();

await builder.Build().RunAsync();

using Blazored.LocalStorage;
using Blazorise;
using Bookify.Client;
using Bookify.Client.Auth;
using Bookify.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ── Auth ──────────────────────────────────────────────────────────────────────
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("StaffOnly", p => p.RequireRole("Staff"));
    options.AddPolicy("StaffOrAdmin", p => p.RequireRole("Staff", "Admin"));
    options.AddPolicy("ClientOnly", p => p.RequireRole("Client"));
});
builder.Services.AddScoped<AuthenticationStateProvider, BookifyAuthStateProvider>();

// ── HTTP Client ───────────────────────────────────────────────────────────────
// AuthBearerHandler injects the JWT Bearer token on every outgoing request by
// reading from local-storage and setting it on the request message (NOT on
// DefaultRequestHeaders), making it safe for concurrent requests.
builder.Services.AddScoped<AuthBearerHandler>();
builder.Services.AddScoped<RetryHandler>();
builder.Services.AddScoped(sp =>
{
    var innerHandler  = new HttpClientHandler();
    var bearerHandler = sp.GetRequiredService<AuthBearerHandler>();
    var retryHandler  = sp.GetRequiredService<RetryHandler>();
    bearerHandler.InnerHandler = innerHandler;
    retryHandler.InnerHandler  = bearerHandler;

    return new HttpClient(retryHandler)
    {
        BaseAddress = new Uri(
            builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7031/")
    };
});

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService,            AuthService>();
builder.Services.AddScoped<IBookingService,         BookingService>();
builder.Services.AddScoped<ICategoryService,        CategoryService>();
builder.Services.AddScoped<IServiceApiService,      ServiceApiService>();
builder.Services.AddScoped<IServiceApprovalApiService, ServiceApprovalApiService>();
builder.Services.AddScoped<IFAQApiService,          FAQApiService>();
builder.Services.AddScoped<ITicketApiService,       TicketApiService>();
builder.Services.AddScoped<IContactInfoApiService,  ContactInfoApiService>();
builder.Services.AddScoped<IStatisticsApiService,   StatisticsApiService>();
builder.Services.AddScoped<IUserApiService,         UserApiService>();
builder.Services.AddScoped<IReviewApiService,       ReviewApiService>();
builder.Services.AddScoped<IProfileApiService,      ProfileApiService>();
builder.Services.AddScoped<IPaymentApiService,      PaymentApiService>();
builder.Services.AddScoped<ToastService>();

await builder.Build().RunAsync();

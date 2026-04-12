using Bookify.API.Middleware;
using Bookify.API.Filters;
using Bookify.Application.Interfaces;
using Bookify.Application;
using Bookify.Infrastructure;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Serilog;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// ============================
// Serilog Configuration
// ============================
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Bookify.API")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/bookify-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}");
});

// ============================
// Controllers with JSON Configuration
// ============================
builder.Services.AddControllers(options => 
    {
        options.Filters.Add<ValidationFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Suppress default model state validation since we use FluentValidation
        options.SuppressModelStateInvalidFilter = true;
    })
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        opt.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.AddServiceDefaults();

// Add services to the container.

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bookify API",
        Version = "v1"
    });

    // JWT Bearer definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your Bearer token in the form 'Bearer {token}'"
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            // Use SecuritySchemeReference for .NET 10+
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});


// ============================
// Application & Infrastructure Services
// ============================
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// ============================
// Firebase Initialization
// ============================
var firebaseServiceAccountPath = builder.Configuration["Firebase:ServiceAccountKeyPath"];
if (!string.IsNullOrEmpty(firebaseServiceAccountPath) && File.Exists(firebaseServiceAccountPath))
{
    using (var stream = new FileStream(firebaseServiceAccountPath, FileMode.Open, FileAccess.Read))
    {
        FirebaseApp.Create(new AppOptions
        {
            Credential = CredentialFactory.FromStream<ServiceAccountCredential>(stream).ToGoogleCredential()
        });
    }
}
else
{
    // Make sure to log via Serilog if it's already configured or statically.
    Log.Warning("Firebase Service Account JSON file not found at {Path}. Push notifications will be disabled.", firebaseServiceAccountPath ?? "null");
}


// Background Services
builder.Services.AddHostedService<Bookify.API.Services.NotificationCleanupService>();

// ============================
// Authorization Policies
// ============================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",      p => p.RequireRole("Admin"));
    options.AddPolicy("StaffOnly",      p => p.RequireRole("Staff"));
    options.AddPolicy("StaffOrAdmin",   p => p.RequireRole("Staff", "Admin"));
    options.AddPolicy("ClientOnly",     p => p.RequireRole("Client"));
});

// ============================
// Forwarded Headers (Essential for Proxy Hosting like runasp.net)
// ============================
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Clear();
});

// ============================
// CORS Configuration
// ============================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("AllowedOrigins")
            .Get<string[]>() ?? new[] { "https://localhost:5138", "https://bookify-dev.netlify.app","http://bookify-dev.netlify.app/", "http://localhost:5138" };

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .AllowCredentials()
            .WithExposedHeaders("Token-Expired");
    });
});

// ============================
// Rate Limiting
// ============================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        // Apply strict limit (10 requests / min) for auth endpoints to prevent brute-force
        if (context.Request.Path.StartsWithSegments("/api/auth", StringComparison.OrdinalIgnoreCase))
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 10,
                    QueueLimit = 0,
                    Window = TimeSpan.FromMinutes(1)
                });
        }

        // Standard global limit for other endpoints (100 rq / min)
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    options.OnRejected = async (context, _) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests. Please try again later."
        });
    };
});

// ============================
// Response Compression
// ============================
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

// ============================
// Middleware Pipeline
// ============================

// Forwarded Headers (MUST be first)
app.UseForwardedHeaders();

// Global Exception Handler (FIRST - to catch all exceptions)
app.UseGlobalExceptionHandler(app.Environment);

// Custom Security Headers
app.UseMiddleware<SecurityHeadersMiddleware>();

// Serilog Request Logging
app.UseSerilogRequestLogging();

// Default endpoints from Aspire
app.MapDefaultEndpoints();

// Swagger enabled in all environments for API access
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bookify API v1");
    c.DocumentTitle = "Bookify API Documentation";
});

// HSTS strictly enforced in production
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// HTTPS Redirection
app.UseHttpsRedirection();

// Response Compression
app.UseResponseCompression();

// CORS
app.UseCors();

// Rate Limiting
app.UseRateLimiter();

// ============================
// Database Migrations & Seeding
// ============================
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (context.Database.GetPendingMigrations().Any())
        {
            Log.Information("Applying pending migrations...");
            await context.Database.MigrateAsync();
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while applying migrations.");
    }

    var seeder = scope.ServiceProvider.GetRequiredService<IIdentitySeeder>();
    await seeder.SeedAsync();
}
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
    await seeder.SeedDatabase();
}
// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// ============================
// Run Application
// ============================
try
{
    Log.Information("Starting Bookify API");
    
    // Explicitly listen on the port provided by Render
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    app.Run($"http://0.0.0.0:{port}");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

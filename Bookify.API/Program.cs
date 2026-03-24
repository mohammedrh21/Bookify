using Bookify.API.Middleware;
using Bookify.Application.Interfaces;
using Bookify.Application;
using Bookify.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Serilog;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

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
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        opt.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();

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
// CORS Configuration
// ============================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("AllowedOrigins")
            .Get<string[]>() ?? new[] { "https://localhost:5138", "http://localhost:5138" };

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

// Global Exception Handler (FIRST - to catch all exceptions)
app.UseGlobalExceptionHandler(app.Environment);

// Custom Security Headers
app.UseMiddleware<SecurityHeadersMiddleware>();

// Serilog Request Logging
app.UseSerilogRequestLogging();

// Default endpoints from Aspire
app.MapDefaultEndpoints();

// Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bookify API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
        c.DocumentTitle = "Bookify API Documentation";
    });
}
else
{
    // HSTS strictly enforced in production
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
// Seed Identity Data
// ============================
using (var scope = app.Services.CreateScope())
{
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
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

using AutoMapper.Features;
using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Application.Interfaces.Auth;
using Bookify.Application.Interfaces.Category;
using Bookify.Application.Interfaces.Client;
using Bookify.Application.Interfaces.Service;
using Bookify.Application.Interfaces.Staff;
using Bookify.Application.Mapping;
using Bookify.Application.Services;
using Bookify.Application.Validators;
using Bookify.Domain.Contracts;
using Bookify.Domain.Contracts.Booking;
using Bookify.Domain.Contracts.Category;
using Bookify.Domain.Contracts.RefreshToken;
using Bookify.Domain.Contracts.Service;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
using Bookify.Infrastructure.Identity;
using Bookify.Infrastructure.Identity.Entity;
using Bookify.Infrastructure.Repositories;
using Bookify.Infrastructure.Service;
using Bookify.Infrastructure.Services.Auth;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Bookify.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddDatabase(services, configuration);
        AddIdentity(services);
        AddAuthentication(services, configuration);
        AddAuthorization(services);
        AddRepositories(services);
        AddApplicationServices(services, configuration);
        AddValidation(services);

        services.AddHttpContextAccessor();
        services.AddScoped<IIdentitySeeder, IdentitySeeder>();

        return services;
    }

    // ============================
    // Database
    // ============================
    private static void AddDatabase(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("MssqlConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    sql.CommandTimeout(30);
                })
            .ConfigureWarnings(w =>
                w.Log(RelationalEventId.PendingModelChangesWarning))
            .EnableSensitiveDataLogging(false) // Disable in production
            .EnableDetailedErrors(false) // Disable in production
        );
    }

    // ============================
    // Identity
    // ============================
    private static void AddIdentity(IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationIdentityUser>(opt =>
        {
            // Password settings - Enhanced security
            opt.Password.RequireDigit = true;
            opt.Password.RequireNonAlphanumeric = true; // FIXED: Changed from false
            opt.Password.RequiredLength = 12; // FIXED: Increased from 8
            opt.Password.RequireLowercase = true;
            opt.Password.RequireUppercase = true;

            // User settings
            opt.User.RequireUniqueEmail = true; // FIXED: Added
            opt.SignIn.RequireConfirmedEmail = false; // Set to true in production

            // Lockout settings
            opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            opt.Lockout.MaxFailedAccessAttempts = 5;
            opt.Lockout.AllowedForNewUsers = true;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager<SignInManager<ApplicationIdentityUser>>()
            .AddDefaultTokenProviders();
    }

    // ============================
    // JWT Authentication
    // ============================
    private static void AddAuthentication(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(opt =>
        {
            opt.SaveToken = true;
            opt.RequireHttpsMetadata = true; // Enforce HTTPS
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidIssuer = configuration["Jwt:Issuer"],
                ClockSkew = TimeSpan.FromSeconds(30), // FIXED: Reduced from 10 to 30 for production
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
            };

            opt.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        error = "You are not authorized to access this resource"
                    });
                    return context.Response.WriteAsync(result);
                }
            };
        });
    }

    // ============================
    // Authorization
    // ============================
    private static void AddAuthorization(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", p => p.RequireRole(Roles.Admin));
            options.AddPolicy("StaffOnly", p => p.RequireRole(Roles.Staff));
            options.AddPolicy("ClientOnly", p => p.RequireRole(Roles.Client));
            options.AddPolicy("StaffOrAdmin", p => p.RequireRole(Roles.Staff, Roles.Admin));
            options.AddPolicy("ClientOrAdmin", p => p.RequireRole(Roles.Client, Roles.Admin));
        });
    }

    // ============================
    // Repositories
    // ============================
    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>(); // FIXED: Added
    }

    // ============================
    // Application Services
    // ============================
    private static void AddApplicationServices(IServiceCollection services, IConfiguration configuration)
    {

        services.AddAutoMapper(cfg =>
        {
            if (!string.IsNullOrEmpty(configuration["AutoMapper:LicenseKey"]))
            {
                cfg.LicenseKey = configuration["AutoMapper:LicenseKey"];
            }
            cfg.AddProfile<MappingConfig>();

        }, AppDomain.CurrentDomain.GetAssemblies());


        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IServiceService, ServiceService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IGenericRepository<Client>, GenericRepository<Client>>();
    }

    // ============================
    // Validation
    // ============================
    private static void AddValidation(IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
    }
}


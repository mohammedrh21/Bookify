using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Application.Interfaces.Auth;
using Bookify.Application.Interfaces.Category;
using Bookify.Application.Interfaces.Client;
using Bookify.Application.Interfaces.Service;
using Bookify.Application.Interfaces.Staff;
using Bookify.Application.Mapping;
using Bookify.Application.Services;
using Bookify.Domain.Contracts.Booking;
using Bookify.Domain.Contracts.Category;
using Bookify.Domain.Contracts.Service;
using Bookify.Infrastructure.Data;
using Bookify.Infrastructure.Identity;
using Bookify.Infrastructure.Identity.Entity;
using Bookify.Infrastructure.Repositories;
using Bookify.Infrastructure.Service;
using Bookify.Infrastructure.Services;
using Bookify.Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        AddApplicationServices(services);

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
                    sql.EnableRetryOnFailure();
                })
            .ConfigureWarnings(w =>
                w.Log(RelationalEventId.PendingModelChangesWarning))
        );
    }

    // ============================
    // Identity
    // ============================
    private static void AddIdentity(IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationIdentityUser>(opt =>
        {
            opt.Password.RequireDigit = true;
            opt.Password.RequireNonAlphanumeric = false;
            opt.Password.RequiredLength = 8;
            opt.Password.RequireLowercase = true;
            opt.Password.RequireUppercase = true;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager<SignInManager<ApplicationIdentityUser>>();
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
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidIssuer = configuration["Jwt:Issuer"],
                ClockSkew = TimeSpan.FromSeconds(10),
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
            };
            opt.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine("AUTH FAILED: " + context.Exception.Message);
                    return Task.CompletedTask;
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
    }
    // ============================
    // Application Services
    // ============================
    private static void AddApplicationServices(IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingConfig>());
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IServiceService, ServiceService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICategoryService, CategoryService>();

    }
}

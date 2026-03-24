using Bookify.Application.Interfaces;
using Bookify.Application.Interfaces.Category;
using Bookify.Application.Interfaces.ContactInfo;
using Bookify.Application.Interfaces.FAQ;
using Bookify.Application.Interfaces.Service;
using Bookify.Application.Interfaces.Ticket;
using Bookify.Application.Interfaces.Users;
using Bookify.Application.Mapping;
using Bookify.Application.Services;
using Bookify.Application.Services.Users;
using Bookify.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Bookify.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddAutoMapper(services, configuration);
        AddServices(services);
        AddValidation(services);

        return services;
    }

    private static void AddAutoMapper(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(cfg =>
        {
            if (!string.IsNullOrEmpty(configuration["AutoMapper:LicenseKey"]))
            {
                cfg.LicenseKey = configuration["AutoMapper:LicenseKey"];
            }
            cfg.AddProfile<MappingConfig>();
        }, AppDomain.CurrentDomain.GetAssemblies());
    }

    private static void AddServices(IServiceCollection services)
    {
        // Application Services (Implemented in Application Layer)
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IServiceService, ServiceService>();
        services.AddScoped<IServiceApprovalService, ServiceApprovalService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IFAQService, FAQService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IContactInfoService, ContactInfoService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IUserService, UserService>();
    }

    private static void AddValidation(IServiceCollection services)
    {
        // FluentValidation triggers are usually registered in API,
        // but the validators themselves belong to the Application Assembly.
        services.AddValidatorsFromAssemblyContaining<CreateBookingRequestValidator>(); 
    }
}

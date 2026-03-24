using Bookify.Application.Interfaces.Auth;
using Bookify.Application.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace Bookify.Infrastructure.Services.Auth;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            // Usually NameIdentifier is used for the UserId claim
            var id = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(id, out var guid))
                return guid;
                
            // Fallback to "userId" claim if custom
            id = _httpContextAccessor.HttpContext?.User?.FindFirstValue("userId");
            return Guid.TryParse(id, out var fallbackGuid) ? fallbackGuid : null;
        }
    }

    public string? UserRole => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

    public bool IsAdmin => UserRole == "Admin";
    public bool IsStaff => UserRole == "Staff";
    public bool IsClient => UserRole == "Client";
}

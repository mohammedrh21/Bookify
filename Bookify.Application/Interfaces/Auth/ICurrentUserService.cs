using System;

namespace Bookify.Application.Interfaces.Auth;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserRole { get; }
    bool IsAdmin { get; }
    bool IsStaff { get; }
    bool IsClient { get; }
}

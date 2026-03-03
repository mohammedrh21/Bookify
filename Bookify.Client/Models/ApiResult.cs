namespace Bookify.Client.Models;

/// <summary>
/// Thin result carrier returned by all client service methods.
/// Success = true  → <c>Data</c> is populated, <c>Message</c> is the confirmation text.
/// Success = false → <c>Message</c> contains the user-friendly error description.
/// </summary>
public record ApiResult<T>(bool Success, string? Message, T? Data = default)
{
    public static ApiResult<T> Ok(T? data = default, string? message = null)
        => new(true, message, data);

    public static ApiResult<T> Fail(string message)
        => new(false, message);
}

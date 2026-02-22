namespace Bookify.Client.Models;

/// <summary>
/// Mirrors the API's <c>ServiceResponse&lt;T&gt;</c> envelope.
/// </summary>
public class ApiResponse<T>
{
    public bool    Success { get; set; }
    public string? Message { get; set; }
    public T?      Data    { get; set; }
    public Guid?   Id      { get; set; }
}

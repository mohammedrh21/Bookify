using Blazored.LocalStorage;
using Bookify.Client.Auth;
using Bookify.Client.Models;
using Bookify.Client.Models.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace Bookify.Client.Services;

// ── Interface ────────────────────────────────────────────────────────────────
public interface IAuthService
{
    Task<(bool Success, string? Message)> LoginAsync(LoginRequest request);
    Task<(bool Success, string? Message)> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task<string?> GetUserRoleAsync();
    Task<Guid?> GetUserIdAsync();
}

// ── Implementation ───────────────────────────────────────────────────────────
public class AuthService(
    HttpClient http,
    ILocalStorageService localStorage,
    AuthenticationStateProvider authStateProvider) : IAuthService
{
    public async Task<(bool Success, string? Message)> LoginAsync(LoginRequest request)
    {
        var httpResponse = await http.PostAsJsonAsync("api/auth/login", request);
        var result = await httpResponse.Content
            .ReadFromJsonAsync<ApiResponse<LoginResponseModel>>();

        if (result is null)
            return (false, "Unknown error — empty response.");

        if (!result.Success || result.Data is null)
            return (false, result.Message ?? "Login failed.");

        var login = result.Data;

        await localStorage.SetItemAsync("access_token", login.AccessToken);
        await localStorage.SetItemAsync("refresh_token", login.RefreshToken);
        await localStorage.SetItemAsync("user_name", login.FullName);
        await localStorage.SetItemAsync("user_role", login.Role);
        await localStorage.SetItemAsync("user_id", login.UserId.ToString());
       

            ((BookifyAuthStateProvider)authStateProvider)
                .NotifyUserAuthenticated(login.AccessToken);

        return (true, null);
    }

    public async Task<(bool Success, string? Message)> RegisterAsync(RegisterRequest request)
    {
        // API endpoint is /api/auth/register/client
        var httpResponse = await http.PostAsJsonAsync("api/auth/register/client", new
        {
            request.FullName,
            request.Email,
            request.Password,
            request.Phone,
            request.DateOfBirth
        });

        var result = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();

        if (result is null)
            return (false, "Unknown error — empty response.");

        return result.Success
            ? (true, result.Message)
            : (false, result.Message ?? "Registration failed.");
    }

    public async Task LogoutAsync()
    {
        await localStorage.RemoveItemAsync("access_token");
        await localStorage.RemoveItemAsync("refresh_token");
        await localStorage.RemoveItemAsync("user_name");
        await localStorage.RemoveItemAsync("user_role");
        await localStorage.RemoveItemAsync("user_id");
        ((BookifyAuthStateProvider)authStateProvider).NotifyUserLogout();
    }

    public async Task<string?> GetTokenAsync()
        => await localStorage.GetItemAsync<string>("access_token");

    public async Task<string?> GetUserRoleAsync()
        => await localStorage.GetItemAsync<string>("user_role");

    public async Task<Guid?> GetUserIdAsync()
    {
        var raw = await localStorage.GetItemAsync<string>("user_id");
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}

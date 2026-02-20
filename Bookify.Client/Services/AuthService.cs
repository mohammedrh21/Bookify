using Blazored.LocalStorage;
using Bookify.Client.Auth;
using Bookify.Client.Models.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace Bookify.Client.Services;

// ── Interface ────────────────────────────────────────────────────────────────
public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task<string?> GetUserRoleAsync();
}

// ── Implementation ───────────────────────────────────────────────────────────
public class AuthService(
    HttpClient http,
    ILocalStorageService localStorage,
    AuthenticationStateProvider authStateProvider) : IAuthService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var response = await http.PostAsJsonAsync("api/auth/login", request);
        var result   = await response.Content.ReadFromJsonAsync<AuthResponse>()
                       ?? new AuthResponse { IsSuccess = false, Message = "Unknown error" };

        if (result.IsSuccess)
        {
            await localStorage.SetItemAsync("access_token",  result.AccessToken);
            await localStorage.SetItemAsync("refresh_token", result.RefreshToken);
            await localStorage.SetItemAsync("user_name",     result.FullName);
            await localStorage.SetItemAsync("user_role",     result.Role);

            ((BookifyAuthStateProvider)authStateProvider)
                .NotifyUserAuthenticated(result.AccessToken);
        }

        return result;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await http.PostAsJsonAsync("api/auth/register", request);
        return await response.Content.ReadFromJsonAsync<AuthResponse>()
               ?? new AuthResponse { IsSuccess = false, Message = "Unknown error" };
    }

    public async Task LogoutAsync()
    {
        await localStorage.RemoveItemAsync("access_token");
        await localStorage.RemoveItemAsync("refresh_token");
        await localStorage.RemoveItemAsync("user_name");
        await localStorage.RemoveItemAsync("user_role");
        ((BookifyAuthStateProvider)authStateProvider).NotifyUserLogout();
    }

    public async Task<string?> GetTokenAsync()
        => await localStorage.GetItemAsync<string>("access_token");

    public async Task<string?> GetUserRoleAsync()
        => await localStorage.GetItemAsync<string>("user_role");
}

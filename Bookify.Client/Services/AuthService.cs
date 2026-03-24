using Blazored.LocalStorage;
using Bookify.Client.Models;
using Bookify.Client.Models.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Text.Json;

namespace Bookify.Client.Services;

// ── Interface ────────────────────────────────────────────────────────────────
public interface IAuthService
{
    Task<ApiResult<LoginResponseModel>> LoginAsync(LoginRequest request);
    Task<ApiResult<Guid>> RegisterClientAsync(RegisterRequest request);
    Task<ApiResult<Guid>> RegisterStaffAsync(RegisterRequest request);

    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task<string?> GetUserRoleAsync();
    Task<Guid?> GetUserIdAsync();

    // Forgot Password
    Task<ApiResult<bool>> ForgotPasswordAsync(ForgotPasswordRequestModel request);
    Task<ApiResult<string>> VerifyOtpAsync(VerifyOtpRequestModel request);
    Task<ApiResult<bool>> ResetPasswordAsync(ResetPasswordRequestModel request);
}

// ── Implementation ───────────────────────────────────────────────────────────
public class AuthService(
    HttpClient http,
    ILocalStorageService localStorage,
    AuthenticationStateProvider authStateProvider,
    ToastService toast) : IAuthService
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Reads the error body from a failed HTTP response.
    /// The API always returns { "message": "..." } on error via GlobalExceptionMiddleware.
    /// </summary>
    private static async Task<List<string>> ReadErrorMessagesAsync(HttpResponseMessage response, string fallback)
    {
        var errors = new List<string>();
        try
        {
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            // 1. Check for RFC 9110 validation errors dict
            if (json.TryGetProperty("errors", out var errorsDict) && errorsDict.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in errorsDict.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var errStr in prop.Value.EnumerateArray())
                        {
                            if (errStr.ValueKind == JsonValueKind.String)
                            {
                                var val = errStr.GetString();
                                if (!string.IsNullOrWhiteSpace(val)) errors.Add(val);
                            }
                        }
                    }
                }
            }

            // 2. Fallback to standard message property
            if (errors.Count == 0 && json.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
            {
                var val = msg.GetString();
                if (!string.IsNullOrWhiteSpace(val)) errors.Add(val);
            }
        }
        catch { }

        if (errors.Count == 0) errors.Add(fallback);
        return errors;
    }

    private void ShowErrors(List<string> errors)
    {
        foreach (var error in errors)
        {
            toast.ShowError(error);
        }
    }

    // ── Auth operations ──────────────────────────────────────────────────────

    public async Task<ApiResult<LoginResponseModel>> LoginAsync(LoginRequest request)
    {
        var httpResponse = await http.PostAsJsonAsync("api/auth/login", request);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errors = await ReadErrorMessagesAsync(httpResponse, "Login failed. Please check your credentials.");
            return ApiResult<LoginResponseModel>.Fail(errors.FirstOrDefault() ?? "Error");
        }

        var result = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponseModel>>();
        if (result?.Data is null)
        {
            const string msg = "Login failed — unexpected server response.";
            return ApiResult<LoginResponseModel>.Fail(msg);
        }

        var login = result.Data;
        await localStorage.SetItemAsync("access_token",  login.AccessToken);
        await localStorage.SetItemAsync("refresh_token", login.RefreshToken);
        await localStorage.SetItemAsync("user_name",     login.FullName);
        await localStorage.SetItemAsync("user_role",     login.Role);
        await localStorage.SetItemAsync("user_id",       login.UserId.ToString());

        ((Auth.BookifyAuthStateProvider)authStateProvider)
            .NotifyUserAuthenticated(login.AccessToken);

        return ApiResult<LoginResponseModel>.Ok(login, result.Message);
    }

    public async Task<ApiResult<Guid>> RegisterClientAsync(RegisterRequest request)
    {
        var httpResponse = await http.PostAsJsonAsync("api/auth/register/client", new
        {
            request.FullName,
            request.Email,
            request.Password,
            request.Phone,
            request.DateOfBirth
        });

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errors = await ReadErrorMessagesAsync(httpResponse, "Registration failed. Please try again.");
            return ApiResult<Guid>.Fail(errors.FirstOrDefault() ?? "Error");
        }

        var result = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var msg = result?.Message ?? "Client registered successfully.";
        return ApiResult<Guid>.Ok(result?.Id ?? Guid.Empty, msg);
    }

    public async Task<ApiResult<Guid>> RegisterStaffAsync(RegisterRequest request)
    {
        var httpResponse = await http.PostAsJsonAsync("api/auth/register/staff", new
        {
            request.FullName,
            request.Email,
            request.Password,
            request.Phone
        });

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errors = await ReadErrorMessagesAsync(httpResponse, "Registration failed. Please try again.");
            return ApiResult<Guid>.Fail(errors.FirstOrDefault() ?? "Error");
        }

        var result = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var msg = result?.Message ?? "Staff registered successfully.";
        return ApiResult<Guid>.Ok(result?.Id ?? Guid.Empty, msg);
    }

    public async Task LogoutAsync()
    {
        await localStorage.RemoveItemAsync("access_token");
        await localStorage.RemoveItemAsync("refresh_token");
        await localStorage.RemoveItemAsync("user_name");
        await localStorage.RemoveItemAsync("user_role");
        await localStorage.RemoveItemAsync("user_id");
        ((Auth.BookifyAuthStateProvider)authStateProvider).NotifyUserLogout();
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

    public async Task<ApiResult<bool>> ForgotPasswordAsync(ForgotPasswordRequestModel request)
    {
        var httpResponse = await http.PostAsJsonAsync("api/auth/forgot-password", request);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errors = await ReadErrorMessagesAsync(httpResponse, "Failed to initiate password reset.");
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }

        var result = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        return ApiResult<bool>.Ok(true, result?.Message);
    }

    public async Task<ApiResult<string>> VerifyOtpAsync(VerifyOtpRequestModel request)
    {
        var httpResponse = await http.PostAsJsonAsync("api/auth/verify-otp", request);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errors = await ReadErrorMessagesAsync(httpResponse, "OTP verification failed.");
            return ApiResult<string>.Fail(errors.FirstOrDefault() ?? "Error");
        }

        var result = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<string>>();
        return ApiResult<string>.Ok(result?.Data ?? string.Empty, result?.Message);
    }

    public async Task<ApiResult<bool>> ResetPasswordAsync(ResetPasswordRequestModel request)
    {
        var httpResponse = await http.PostAsJsonAsync("api/auth/reset-password", request);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errors = await ReadErrorMessagesAsync(httpResponse, "Password reset failed.");
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }

        var result = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        return ApiResult<bool>.Ok(true, result?.Message);
    }
}

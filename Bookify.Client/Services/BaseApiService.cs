using System.Net.Http.Json;
using System.Text.Json;
using Bookify.Client.Models;
using Bookify.Client.Models.Common;
using Blazorise;

namespace Bookify.Client.Services;

/// <summary>
/// Abstract base class for all API service classes.
/// Provides the shared ReadErrorsAsync and ShowErrors helpers that were
/// previously copy-pasted verbatim into every service file.
/// </summary>
public abstract class BaseApiService(HttpClient http, ToastService toast)
{
    protected readonly HttpClient Http = http;
    protected readonly ToastService Toast = toast;

    /// <summary>Shows all errors as individual error toasts.</summary>
    protected void ShowErrors(List<string> errors)
    {
        foreach (var error in errors)
            Toast.ShowError(error);
    }

    protected void ShowSuccess(string message)
        => Toast.ShowSuccess(message);

    // ── Generic HTTP Helpers ─────────────────────────────────────────────

    protected async Task<ApiResult<TResponse?>> GetAsync<TResponse>(string url, string fallbackError)
    {
        var response = await Http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, fallbackError);
            return ApiResult<TResponse?>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>();
        return ApiResult<TResponse?>.Ok(result != null ? result.Data : default);
    }

    protected async Task<ApiResult<TResponse?>> GetDirectAsync<TResponse>(string url, string fallbackError)
    {
        var response = await Http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, fallbackError);
            return ApiResult<TResponse?>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<TResponse>();
        return ApiResult<TResponse?>.Ok(result);
    }

    protected async Task<ApiResult<bool>> PostAsync<TRequest>(string url, TRequest request, string fallbackError)
    {
        var response = await Http.PostAsJsonAsync(url, request);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, fallbackError);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        var msg = result?.Message ?? "Operation successful.";
        return ApiResult<bool>.Ok(true, msg);
    }

    protected async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest request, string fallbackError)
    {
        var response = await Http.PostAsJsonAsync(url, request);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, fallbackError);
            return ApiResult<TResponse>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>();
        var msg = result?.Message ?? "Operation successful.";
        return ApiResult<TResponse>.Ok(result != null ? result.Data : default!, msg);
    }

    protected async Task<ApiResult<string>> PostMultipartAsync(string url, MultipartFormDataContent request, string fallbackError)
    {
        var response = await Http.PostAsync(url, request);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, fallbackError);
            return ApiResult<string>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
        return ApiResult<string>.Ok(result?.Data ?? string.Empty, result?.Message);
    }

    protected async Task<ApiResult<bool>> PutAsync<TRequest>(string url, TRequest request, string fallbackError)
    {
        var response = await Http.PutAsJsonAsync(url, request);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, fallbackError);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        var msg = result?.Message ?? "Operation successful.";
        return ApiResult<bool>.Ok(true, msg);
    }

    protected async Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(string url, TRequest request, string fallbackError)
    {
        var response = await Http.PutAsJsonAsync(url, request);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, fallbackError);
            return ApiResult<TResponse>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>();
        var msg = result?.Message ?? "Operation successful.";
        return ApiResult<TResponse>.Ok(result != null ? result.Data : default!, msg);
    }

    protected async Task<ApiResult<bool>> PatchAsync<TRequest>(string url, TRequest request, string fallbackError)
    {
        var response = await Http.PatchAsJsonAsync(url, request);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, fallbackError);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        var msg = result?.Message ?? "Operation successful.";
        return ApiResult<bool>.Ok(true, msg);
    }

    protected async Task<ApiResult<bool>> DeleteAsync(string url, string fallbackError)
    {
        var response = await Http.DeleteAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, fallbackError);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        var msg = result?.Message ?? "Operation successful.";
        return ApiResult<bool>.Ok(true, msg);
    }

    // ── Core Message Parsers ─────────────────────────────────────────────

    /// <summary>
    /// Parses error messages from a failed HTTP response.
    /// Supports RFC 9110 validation-errors object and a plain "message" property.
    /// </summary>
    protected static async Task<List<string>> ReadErrorsAsync(
        HttpResponseMessage response,
        string fallback)
    {
        var errors = new List<string>();
        try
        {
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            // 1. RFC 9110 validation-errors dict: { "errors": { "Field": ["msg"] } }
            if (json.TryGetProperty("errors", out var errorsDict)
                && errorsDict.ValueKind == JsonValueKind.Object)
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

            // 2. Plain message property: { "message": "..." }
            if (errors.Count == 0
                && json.TryGetProperty("message", out var msg)
                && msg.ValueKind == JsonValueKind.String)
            {
                var val = msg.GetString();
                if (!string.IsNullOrWhiteSpace(val)) errors.Add(val);
            }
        }
        catch { /* malformed JSON — fall through to fallback */ }

        if (errors.Count == 0) errors.Add(fallback);
        return errors;
    }
}

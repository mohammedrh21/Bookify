using Bookify.Client.Models;
using Bookify.Client.Models.ContactInfo;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bookify.Client.Services;

public class ContactInfoApiService : IContactInfoApiService
{
    private readonly HttpClient _httpClient;
    private readonly ToastService _toastService;

    public ContactInfoApiService(HttpClient httpClient, ToastService toastService)
    {
        _httpClient = httpClient;
        _toastService = toastService;
    }

    public async Task<ApiResult<ContactInfoModel>> GetAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/contact-info");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return ApiResult<ContactInfoModel>.Ok(null, "No contact info configured yet.");

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                options.Converters.Add(new JsonStringEnumConverter());

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ContactInfoModel>>(options);

                if (apiResponse != null && apiResponse.Success)
                    return ApiResult<ContactInfoModel>.Ok(apiResponse.Data);

                return ApiResult<ContactInfoModel>.Fail(apiResponse?.Message ?? "Failed to fetch contact info.");
            }

            return ApiResult<ContactInfoModel>.Fail($"Server error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResult<ContactInfoModel>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResult<Guid>> CreateAsync(ContactInfoModel request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/contact-info", request);
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
                if (apiResponse != null && apiResponse.Success)
                {
                    _toastService.ShowSuccess("Contact Info created successfully!");
                    return ApiResult<Guid>.Ok(apiResponse.Data, apiResponse.Message);
                }
                _toastService.ShowError(apiResponse?.Message ?? "Failed to create contact info.");
                return ApiResult<Guid>.Fail(apiResponse?.Message ?? "Failed to create contact info.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _toastService.ShowError("Error creating contact info. Check your inputs.");
            return ApiResult<Guid>.Fail($"Server error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"An error occurred: {ex.Message}");
            return ApiResult<Guid>.Fail($"Exception: {ex.Message}");
        }
    }

    public async Task<ApiResult<Guid>> UpdateAsync(ContactInfoModel request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/contact-info/{request.Id}", request);
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
                if (apiResponse != null && apiResponse.Success)
                {
                    _toastService.ShowSuccess("Contact Info updated successfully!");
                    return ApiResult<Guid>.Ok(apiResponse.Data, apiResponse.Message);
                }
                _toastService.ShowError(apiResponse?.Message ?? "Failed to update contact info.");
                return ApiResult<Guid>.Fail(apiResponse?.Message ?? "Failed to update contact info.");
            }

            _toastService.ShowError("Error updating contact info.");
            return ApiResult<Guid>.Fail($"Server error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"An error occurred: {ex.Message}");
            return ApiResult<Guid>.Fail($"Exception: {ex.Message}");
        }
    }
}

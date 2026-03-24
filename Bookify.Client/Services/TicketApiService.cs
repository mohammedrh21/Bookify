using Bookify.Client.Models;
using Bookify.Client.Models.SupportTicket;
using System.Net.Http.Json;

namespace Bookify.Client.Services;

public interface ITicketApiService
{
    Task<ApiResult<PagedResult<TicketModel>>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<ApiResult<bool>>                    SubmitAsync(TicketModel model);
    Task<ApiResult<bool>>                    MarkAsReadAsync(Guid id);
}

public class TicketApiService(HttpClient http, ToastService toast)
    : BaseApiService(http, toast), ITicketApiService
{
    public async Task<ApiResult<PagedResult<TicketModel>>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        var result = await GetAsync<PagedResult<TicketModel>>($"api/tickets?pageNumber={pageNumber}&pageSize={pageSize}", "Failed to load support tickets.");
        return ApiResult<PagedResult<TicketModel>>.Ok(result.Data);
    }

    public async Task<ApiResult<bool>> SubmitAsync(TicketModel model)
    {
        var response = await Http.PostAsJsonAsync("api/tickets", new { model.Email, model.Subject, model.Description });

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            const string msg = "You have already submitted a support ticket today. Please try again tomorrow.";
            ShowErrors([msg]);
            return ApiResult<bool>.Fail(msg);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorsAsync(response, "Failed to submit ticket.");
            ShowErrors(errors);
            return ApiResult<bool>.Fail(errors.FirstOrDefault() ?? "Error");
        }

        return ApiResult<bool>.Ok(true, "Ticket submitted successfully.");
    }

    public async Task<ApiResult<bool>> MarkAsReadAsync(Guid id)
        => await PatchAsync($"api/tickets/{id}/read", (object)null!, "Failed to mark ticket as read.");
}


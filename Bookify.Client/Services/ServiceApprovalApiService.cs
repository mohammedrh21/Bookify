using Bookify.Client.Models;
using Bookify.Client.Models.Service;
using System.Net.Http.Json;

namespace Bookify.Client.Services
{
    public interface IServiceApprovalApiService
    {
        Task<ApiResult<IEnumerable<ServiceApprovalRequestModel>>> GetAllRequestsAsync();
        Task<ApiResult<IEnumerable<ServiceApprovalRequestModel>>> GetMyRequestsAsync();
        Task<ApiResult<bool>> SubmitCreateAsync(ServiceModel model);
        Task<ApiResult<bool>> SubmitUpdateAsync(ServiceModel model);
        Task<ApiResult<bool>> ApproveAsync(Guid requestId);
        Task<ApiResult<bool>> RejectAsync(Guid requestId, string comment);
    }

    public class ServiceApprovalApiService(HttpClient http, ToastService toast)
        : BaseApiService(http, toast), IServiceApprovalApiService
    {
        public async Task<ApiResult<IEnumerable<ServiceApprovalRequestModel>>> GetAllRequestsAsync()
        {
            var result = await GetAsync<IEnumerable<ServiceApprovalRequestModel>>("api/serviceapproval", "Failed to load approval requests.");
            return ApiResult<IEnumerable<ServiceApprovalRequestModel>>.Ok(result.Data ?? []);
        }

        public async Task<ApiResult<IEnumerable<ServiceApprovalRequestModel>>> GetMyRequestsAsync()
        {
            var result = await GetAsync<IEnumerable<ServiceApprovalRequestModel>>("api/serviceapproval/my-requests", "Failed to load your approval requests.");
            return ApiResult<IEnumerable<ServiceApprovalRequestModel>>.Ok(result.Data ?? []);
        }

        public async Task<ApiResult<bool>> SubmitCreateAsync(ServiceModel model)
            => await PostAsync("api/serviceapproval/submit-create", model, "Failed to submit creation request.");

        public async Task<ApiResult<bool>> SubmitUpdateAsync(ServiceModel model)
            => await PostAsync("api/serviceapproval/submit-update", model, "Failed to submit update request.");

        public async Task<ApiResult<bool>> ApproveAsync(Guid requestId)
            => await PostAsync($"api/serviceapproval/{requestId}/approve", (object)null!, "Failed to approve request.");

        public async Task<ApiResult<bool>> RejectAsync(Guid requestId, string comment)
            => await PostAsync($"api/serviceapproval/{requestId}/reject?comment={Uri.EscapeDataString(comment)}", (object)null!, "Failed to reject request.");
    }

}

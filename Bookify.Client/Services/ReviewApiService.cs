using Bookify.Client.Models;
using Bookify.Client.Models.Common;
using System.Net.Http.Json;

namespace Bookify.Client.Services
{
    public interface IReviewApiService
    {
        Task<ApiResult<ReviewModel>>                    CreateReviewAsync(CreateReviewRequest request);
        Task<ApiResult<PagedResult<ReviewModel>>>       GetServiceReviewsAsync(Guid serviceId, int page = 1, int pageSize = 10);
        Task<ApiResult<PagedResult<ReviewModel>>>       GetMyReviewsAsync(int page = 1, int pageSize = 10);
    }

    public class ReviewApiService(HttpClient http, ToastService toast)
        : BaseApiService(http, toast), IReviewApiService
    {
        public async Task<ApiResult<ReviewModel>> CreateReviewAsync(CreateReviewRequest request)
            => await PostAsync<CreateReviewRequest, ReviewModel>("api/reviews", request, "Failed to create review.");

        public async Task<ApiResult<PagedResult<ReviewModel>>> GetServiceReviewsAsync(
            Guid serviceId, int page = 1, int pageSize = 10)
        {
            var url    = $"api/reviews/service/{serviceId}?page={page}&pageSize={pageSize}";
            var result = await GetAsync<PagedResult<ReviewModel>>(url, "Failed to fetch reviews.");
            return ApiResult<PagedResult<ReviewModel>>.Ok(result.Data ?? new PagedResult<ReviewModel>());
        }

        public async Task<ApiResult<PagedResult<ReviewModel>>> GetMyReviewsAsync(int page = 1, int pageSize = 10)
        {
            var url    = $"api/reviews/my-reviews?page={page}&pageSize={pageSize}";
            var result = await GetAsync<PagedResult<ReviewModel>>(url, "Failed to fetch reviews.");
            return ApiResult<PagedResult<ReviewModel>>.Ok(result.Data ?? new PagedResult<ReviewModel>());
        }
    }

}

using Bookify.Application.Common;
using Bookify.Application.DTO.Common;
using Bookify.Application.DTO.Review;

namespace Bookify.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ServiceResponse<ReviewDto>> CreateReviewAsync(CreateReviewRequest request);

        Task<ServiceResponse<PagedResult<ReviewDto>>> GetReviewsByServiceAsync(
            Guid serviceId, int page = 1, int pageSize = 10);

        Task<ServiceResponse<PagedResult<ReviewDto>>> GetReviewsByClientAsync(
            Guid clientId, int page = 1, int pageSize = 10);
    }
}

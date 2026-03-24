using Bookify.Application.Common;
using Bookify.Application.DTO.FAQ;
using Bookify.Application.DTO.Common;

namespace Bookify.Application.Interfaces.FAQ
{
    public interface IFAQService
    {
        Task<ServiceResponse<PagedResult<FAQResponse>>> GetAllAsync(PaginationParams paginationParams);
        Task<ServiceResponse<FAQResponse>> GetByIdAsync(Guid id);
        Task<ServiceResponse<Guid>> CreateAsync(CreateFAQRequest request);
        Task<ServiceResponse<Guid>> UpdateAsync(UpdateFAQRequest request);
        Task<ServiceResponse<Guid>> DeleteAsync(Guid id);
    }
}

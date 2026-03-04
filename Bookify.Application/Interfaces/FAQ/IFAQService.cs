using Bookify.Application.Common;
using Bookify.Application.DTO.FAQ;

namespace Bookify.Application.Interfaces.FAQ
{
    public interface IFAQService
    {
        Task<ServiceResponse<IEnumerable<FAQResponse>>> GetAllAsync();
        Task<ServiceResponse<FAQResponse>> GetByIdAsync(Guid id);
        Task<ServiceResponse<Guid>> CreateAsync(CreateFAQRequest request);
        Task<ServiceResponse<Guid>> UpdateAsync(UpdateFAQRequest request);
        Task<ServiceResponse<Guid>> DeleteAsync(Guid id);
    }
}

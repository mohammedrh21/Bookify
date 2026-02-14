using Bookify.Application.Common;
using Bookify.Application.DTO.Service;

namespace Bookify.Application.Interfaces.Service
{
    public interface IServiceService
    {
        Task<ServiceResponse<Guid>> CreateAsync(CreateServiceRequest request);
        Task<ServiceResponse<bool>> UpdateAsync(UpdateServiceRequest request);
        Task<ServiceResponse<bool>> DeleteAsync(Guid id);
        Task<ServiceResponse<ServiceResponse>> GetByIdAsync(Guid id);
        Task<ServiceResponse<IEnumerable<ServiceResponse>>> GetAllAsync();
    }
}

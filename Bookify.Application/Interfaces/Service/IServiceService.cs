using Bookify.Application.Common;
using Bookify.Application.DTO.Service;

namespace Bookify.Application.Interfaces.Service
{
    public interface IServiceService
    {
        Task<ServiceResponse<Guid>> CreateAsync(CreateServiceRequest request);
        Task<ServiceResponse<Guid>> UpdateAsync(UpdateServiceRequest request);
        Task<ServiceResponse<Guid>> DeleteAsync(Guid id);
        Task<Application.Common.ServiceResponse<ServiceResponse>> GetByIdAsync(Guid id);
        Task<Application.Common.ServiceResponse<IEnumerable<ServiceResponse>>> GetAllAsync();
        Task<Application.Common.ServiceResponse<ServiceResponse>> GetByStaffIdAsync(Guid staffId);
    }
}

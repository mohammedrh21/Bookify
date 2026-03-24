using Bookify.Application.Common;
using Bookify.Application.DTO.Common;
using Bookify.Application.DTO.Service;
using Microsoft.AspNetCore.Http;

namespace Bookify.Application.Interfaces.Service
{
    public interface IServiceService
    {
        Task<ServiceResponse<Guid>> CreateAsync(CreateServiceRequest request);
        Task<ServiceResponse<Guid>> UpdateAsync(UpdateServiceRequest request);
        Task<ServiceResponse<Guid>> DeleteAsync(Guid id);
        Task<Application.Common.ServiceResponse<ServiceResponse>> GetByIdAsync(Guid id);
        Task<Application.Common.ServiceResponse<PagedResult<ServiceResponse>>> GetAllAsync(string? searchTerm = null, int page = 1, int pageSize = 10);
        Task<Application.Common.ServiceResponse<ServiceResponse>> GetByStaffIdAsync(Guid staffId);
        Task<Application.Common.ServiceResponse<string>> UploadServiceImageAsync(Guid serviceId, IFormFile file);
        Task<Common.ServiceResponse<bool>> RemoveServiceImageAsync(Guid serviceId);
        Task<ServiceResponse<bool>> ActivateAsync(Guid id);
        Task<ServiceResponse<bool>> HardDeleteAsync(Guid id);
    }
}

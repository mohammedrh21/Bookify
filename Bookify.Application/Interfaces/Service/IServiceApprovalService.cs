using Bookify.Application.Common;
using Bookify.Application.DTO.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Application.Interfaces.Service
{
    public interface IServiceApprovalService
    {
        Task<ServiceResponse<Guid>> SubmitCreateRequestAsync(CreateServiceRequest request);
        Task<ServiceResponse<Guid>> SubmitUpdateRequestAsync(UpdateServiceRequest request);
        Task<ServiceResponse<IEnumerable<ServiceApprovalRequestResponse>>> GetAllRequestsAsync();
        Task<ServiceResponse<IEnumerable<ServiceApprovalRequestResponse>>> GetRequestsByStaffIdAsync(Guid staffId);
        Task<ServiceResponse<Guid>> ApproveRequestAsync(Guid requestId, Guid adminId);
        Task<ServiceResponse<Guid>> RejectRequestAsync(Guid requestId, Guid adminId, string comment);
    }
}

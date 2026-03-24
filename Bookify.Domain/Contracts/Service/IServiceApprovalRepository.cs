using Bookify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Domain.Contracts.Service
{
    public interface IServiceApprovalRepository
    {
        Task<ServiceApprovalRequest?> GetByIdAsync(Guid id);
        Task<IEnumerable<ServiceApprovalRequest>> GetAllAsync();
        Task<IEnumerable<ServiceApprovalRequest>> GetByStaffIdAsync(Guid staffId);
        Task AddAsync(ServiceApprovalRequest request);
        Task UpdateAsync(ServiceApprovalRequest request);
        Task SaveChangesAsync();
    }
}

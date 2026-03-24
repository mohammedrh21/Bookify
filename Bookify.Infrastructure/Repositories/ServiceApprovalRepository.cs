using Bookify.Domain.Contracts.Service;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookify.Infrastructure.Repositories
{
    public class ServiceApprovalRepository : IServiceApprovalRepository
    {
        private readonly AppDbContext _context;

        public ServiceApprovalRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceApprovalRequest?> GetByIdAsync(Guid id)
        {
            return await _context.ServiceApprovalRequests
                .Include(r => r.Staff)
                .Include(r => r.Service)
                .Include(r => r.Actioner)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<ServiceApprovalRequest>> GetAllAsync()
        {
            return await _context.ServiceApprovalRequests
                .Include(r => r.Staff)
                .Include(r => r.Service)
                .Include(r => r.Actioner)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceApprovalRequest>> GetByStaffIdAsync(Guid staffId)
        {
            return await _context.ServiceApprovalRequests
                .Where(r => r.StaffId == staffId)
                .Include(r => r.Staff)
                .Include(r => r.Service)
                .Include(r => r.Actioner)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(ServiceApprovalRequest request)
        {
            await _context.ServiceApprovalRequests.AddAsync(request);
        }

        public async Task UpdateAsync(ServiceApprovalRequest request)
        {
            _context.ServiceApprovalRequests.Update(request);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

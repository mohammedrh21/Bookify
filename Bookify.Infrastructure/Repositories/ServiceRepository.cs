using Bookify.Domain.Contracts.Service;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _db;
        public ServiceRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(Domain.Entities.Service service)
            => await _db.Services.AddAsync(service);

        public async Task<IEnumerable<Domain.Entities.Service>> GetAllAsync()
            => await _db.Services
                .Include(s => s.Category)
                .Include(s => s.Staff)
                .ToListAsync();

        public async Task<Domain.Entities.Service> GetByIdAsync(Guid id)
            => await _db.Services
                .Include(s => s.Category)
                .Include(s => s.Staff)
                .FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new KeyNotFoundException("Service not found");

        public async Task UpdateAsync(Domain.Entities.Service service)
            => _db.Services.Update(service);

        public async Task<bool> ExistsAsync(string name, Guid staffId)
            => await _db.Services.AnyAsync(s => s.Name == name && s.StaffId == staffId);

        public async Task SaveChangesAsync()
            => await _db.SaveChangesAsync();
    }
}

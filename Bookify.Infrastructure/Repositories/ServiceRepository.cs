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

        public async Task<IEnumerable<Domain.Entities.Service>> GetAllAsync(string? searchTerm = null, int skip = 0, int take = 10)
        {
            var query = _db.Services
                .Include(s => s.Category)
                .Include(s => s.Staff)
                .Where(s => !s.IsDeleted && s.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(term) ||
                    s.Staff.FullName.ToLower().Contains(term));
            }

            return await query
                .OrderBy(s => s.Name)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetCountAsync(string? searchTerm = null)
        {
            var query = _db.Services.Where(s => !s.IsDeleted).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(term) ||
                    s.Staff.FullName.ToLower().Contains(term));
            }

            return await query.CountAsync();
        }

        public async Task<Domain.Entities.Service?> GetByIdAsync(Guid id)
            => await _db.Services
                .Include(s => s.Category)
                .Include(s => s.Staff)
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<Domain.Entities.Service?> GetByStaffIdAsync(Guid staffId)
            => await _db.Services
                .Include(s => s.Category)
                .Include(s => s.Staff)
                .FirstOrDefaultAsync(s => s.StaffId == staffId);

        public async Task UpdateAsync(Domain.Entities.Service service)
            => _db.Services.Update(service);

        public Task RemoveAsync(Domain.Entities.Service service)
        {
            _db.Services.Remove(service);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(string name,Guid staffId)
            => await _db.Services.AnyAsync(s=>s.Name==name && s.StaffId == staffId);

        public async Task SaveChangesAsync()
            => await _db.SaveChangesAsync();
    }
}

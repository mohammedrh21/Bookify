using Bookify.Application.Interfaces.Client;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly AppDbContext _db;

        public ClientRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Client client)
        {
            _db.Clients.Add(client);
            await _db.SaveChangesAsync();
        }

        public async Task<Client?> GetByIdAsync(Guid id)
        {
            return await _db.Clients.FindAsync(id);
        }

        public async Task UpdateAsync(Client client)
        {
            _db.Clients.Update(client);
            await _db.SaveChangesAsync();
        }

        public async Task<(IEnumerable<Client> Items, int TotalCount)> GetClientsPaginatedAsync(int page, int pageSize)
        {
            var query = _db.Clients.AsNoTracking();
            var total = await query.CountAsync();
            var items = await query
                .OrderBy(c => c.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Client?> GetClientWithBookingsAsync(Guid clientId)
        {
            return await _db.Clients
                .Include(c => c.Bookings!)
                   .ThenInclude(b => b.Service)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == clientId);
        }
    }
}

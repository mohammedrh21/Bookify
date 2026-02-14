using Bookify.Application.Interfaces.Client;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;

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
    }
}

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

        //public async Task<Guid> GetByIdentityIdAsync(string identityId)
        //{
        //    Guid Id;
        //    var clientId = await _db.Clients
        //     .Where(c => c.IdentityUserId == identityId)
        //     .Select(c => (Guid?)c.Id)
        //     .FirstOrDefaultAsync();

        //    if (clientId != null)
        //    {
        //        Id = (Guid)clientId;
        //        return Id;
        //    }

        //    var staffId = await _db.Staffs
        //       .Where(s => s.IdentityUserId == identityId)
        //       .Select(s => (Guid?)s.Id)
        //       .FirstOrDefaultAsync();
        //    if (staffId != null)
        //    {
        //        Id = (Guid)staffId;
        //        return Id;
        //    }
                
        //    Id = Guid.Parse(identityId);
        //    return Id;
        //}
    }
}

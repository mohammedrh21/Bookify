using Bookify.Domain.Contracts.SupportTicket;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Infrastructure.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly AppDbContext _db;
        public TicketRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(SupportTicket SupportTicket)
            => await _db.SupportTickets.AddAsync(SupportTicket);

        public async Task<IEnumerable<SupportTicket>> GetAllAsync()
            => _db.SupportTickets.AsNoTracking().ToList();

        public Task<SupportTicket?> GetByIdAsync(Guid id)
            => _db.SupportTickets.SingleOrDefaultAsync(x => x.Id == id);

        public async Task<bool> IsExistsTodayAsync(string email, DateTime date)
            => await _db.SupportTickets.AnyAsync(x => 
                x.Email == email && 
                x.CreatedAt.Date == date.Date);

        public async Task SaveChangesAsync()
            => _db.SaveChanges();
    }
}

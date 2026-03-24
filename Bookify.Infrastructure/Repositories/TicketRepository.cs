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

        public async Task<IEnumerable<SupportTicket>> GetAllAsync()
            => await _db.SupportTickets.AsNoTracking().ToListAsync();

        public async Task<(IEnumerable<SupportTicket> Items, int TotalCount)> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            var query = _db.SupportTickets.AsNoTracking();
            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.CreatedAt)
                                   .Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();
            return (items, totalCount);
        }

        public async Task AddAsync(SupportTicket SupportTicket)
            => await _db.SupportTickets.AddAsync(SupportTicket);

        public Task<SupportTicket?> GetByIdAsync(Guid id)
            => _db.SupportTickets.SingleOrDefaultAsync(x => x.Id == id);

        public async Task<bool> IsExistsTodayAsync(string email, DateTime date)
            => await _db.SupportTickets.AnyAsync(x => 
                x.Email == email && 
                x.CreatedAt.Date == date.Date);

        public async Task SaveChangesAsync()
            => await _db.SaveChangesAsync();
    }
}

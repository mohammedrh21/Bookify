using Bookify.Domain.Contracts.Review;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
        }

        public async Task<bool> HasClientReviewedBookingAsync(Guid bookingId)
        {
            return await _context.Reviews.AnyAsync(r => r.BookingId == bookingId);
        }

        public async Task<IEnumerable<Review>> GetByServiceIdAsync(Guid serviceId, int skip = 0, int take = 20)
        {
            return await _context.Reviews
                .Include(r => r.Client)
                .Include(r => r.Service)
                .Where(r => r.ServiceId == serviceId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetCountByServiceIdAsync(Guid serviceId)
            => await _context.Reviews.CountAsync(r => r.ServiceId == serviceId);

        public async Task<IEnumerable<Review>> GetByClientIdAsync(Guid clientId, int skip = 0, int take = 20)
        {
            return await _context.Reviews
                .Include(r => r.Client)
                .Include(r => r.Service)
                .Where(r => r.ClientId == clientId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetCountByClientIdAsync(Guid clientId)
            => await _context.Reviews.CountAsync(r => r.ClientId == clientId);

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

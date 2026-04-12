using Bookify.Domain.Contracts.Booking;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Repositories
{
    public sealed class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _db;

        public BookingRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Booking booking)
            => await _db.Bookings.AddAsync(booking);

        public Task UpdateAsync(Booking booking)
        {
            _db.Bookings.Update(booking);
            return Task.CompletedTask;
        }

        public async Task<Booking?> GetByIdAsync(Guid id)
            => await _db.Bookings
                .Include(b => b.Service)
                    .ThenInclude(s => s.Staff)
                .Include(b => b.Service)
                    .ThenInclude(s => s.Category)
                .Include(b => b.Client)
                .Include(b => b.Payment)
                .Include(b => b.Review)
                .FirstOrDefaultAsync(b => b.Id == id);

        public async Task<IEnumerable<Booking>> GetByClientIdAsync(Guid clientId, int skip = 0, int take = 10)
            => await _db.Bookings
                .Include(b => b.Service)
                    .ThenInclude(s => s.Staff)
                .Include(b => b.Service)
                    .ThenInclude(s => s.Category)
                .Include(b => b.Client)
                .Include(b => b.Payment)
                .Include(b => b.Review)
                .Where(b => b.ClientId == clientId)
                .OrderByDescending(b => b.Date)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();

        public async Task<IEnumerable<Booking>> GetByStaffIdAsync(Guid staffId, int skip = 0, int take = 10)
            => await _db.Bookings
                .Include(b => b.Service)
                    .ThenInclude(s => s.Staff)
                .Include(b => b.Service)
                    .ThenInclude(s => s.Category)
                .Include(b => b.Client)
                .Include(b => b.Payment)
                .Include(b => b.Review)
                .Where(b => b.Service.StaffId == staffId)
                .OrderByDescending(b => b.Date)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();

        public async Task<IEnumerable<Booking>> GetByStaffIdFilteredAsync(
            Guid staffId,
            BookingStatus? status = null,
            DateTime? from = null,
            DateTime? to = null,
            string? search = null,
            bool sortAscending = true,
            int skip = 0,
            int take = 10)
        {
            var query = _db.Bookings
                .Include(b => b.Service)
                    .ThenInclude(s => s.Staff)
                .Include(b => b.Service)
                    .ThenInclude(s => s.Category)
                .Include(b => b.Client)
                .Include(b => b.Payment)
                .Include(b => b.Review)
                .Where(b => b.Service.StaffId == staffId);

            if (status.HasValue)
                query = query.Where(b => b.Status == status);

            if (from.HasValue)
            {
                var fromDate = from.Value.Date;
                query = query.Where(b => b.Date >= fromDate);
            }

            if (to.HasValue)
            {
                var toDate = to.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(b => b.Date <= toDate);
            }

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(b => b.Client.FullName.Contains(search));

            query = sortAscending
                ? query.OrderBy(b => b.Date).ThenBy(b => b.Time)
                : query.OrderByDescending(b => b.Date).ThenByDescending(b => b.Time);

            return await query
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetByServiceIdAsync(Guid serviceId, DateTime from, DateTime to)
            => await _db.Bookings
                .Where(b => b.ServiceId == serviceId && b.Date >= from && b.Date <= to && b.Status != BookingStatus.Cancelled)
                .AsNoTracking()
                .ToListAsync();

        public async Task<IEnumerable<Booking>> GetStaffDashboardBookingsAsync(Guid staffId, DateTime from, DateTime to)
        {
            var fromDate = from.Date;
            var toDate = to.Date.AddDays(1).AddTicks(-1);

            return await _db.Bookings
                .Include(b => b.Service)
                    .ThenInclude(s => s.Category)
                .Include(b => b.Client)
                .Include(b => b.Payment)
                .Include(b => b.Review)
                .Where(b => b.Service.StaffId == staffId && b.Date.Date >= fromDate && b.Date.Date <= toDate)
                .OrderByDescending(b => b.Date).ThenByDescending(b => b.Time)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.Booking>> GetAdminDashboardBookingsAsync(DateTime? from, DateTime? to)
        {
            var query = _db.Bookings
                .Include(b => b.Service)
                    .ThenInclude(s => s.Staff)
                .Include(b => b.Service)
                    .ThenInclude(s => s.Category)
                .Include(b => b.Client)
                .Include(b => b.Payment)
                .Include(b => b.Review)
                .AsQueryable();

            if (from.HasValue)
            {
                var fromDate = from.Value.Date;
                query = query.Where(b => b.Date >= fromDate);
            }
            
            if (to.HasValue)
            {
                var toDate = to.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(b => b.Date <= toDate);
            }

            return await query
                .OrderByDescending(b => b.Date).ThenByDescending(b => b.Time)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetAllAsync(
            DateTime? from,
            DateTime? to,
            BookingStatus? status,
            string? search = null,
            string? staffNameFilter = null,
            Guid? categoryIdFilter = null,
            int skip = 0,
            int take = 10)
        {
            var query = _db.Bookings
                .Include(b => b.Service)
                    .ThenInclude(s => s.Staff)
                .Include(b => b.Service)
                    .ThenInclude(s => s.Category)
                .Include(b => b.Client)
                .Include(b => b.Payment)
                .Include(b => b.Review)
                .AsQueryable();

            if (from.HasValue)
            {
                var fromDate = from.Value.Date;
                query = query.Where(b => b.Date >= fromDate);
            }

            if (to.HasValue)
            {
                var toDate = to.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(b => b.Date <= toDate);
            }

            if (status.HasValue)
                query = query.Where(b => b.Status == status);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(b =>
                    b.Service.Name.Contains(search) ||
                    b.Client.FullName.Contains(search) ||
                    b.Service.Staff.FullName.Contains(search));

            if (!string.IsNullOrWhiteSpace(staffNameFilter))
                query = query.Where(b => b.Service.Staff.FullName.Contains(staffNameFilter));

            if (categoryIdFilter.HasValue)
                query = query.Where(b => b.Service.CategoryId == categoryIdFilter.Value);

            return await query
                .OrderByDescending(b => b.Date)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetCountAsync(
            DateTime? from = null,
            DateTime? to = null,
            BookingStatus? status = null,
            string? search = null,
            string? staffNameFilter = null,
            Guid? categoryIdFilter = null,
            Guid? clientId = null,
            Guid? staffId = null)
        {
            var query = _db.Bookings
                .Include(b => b.Service)
                    .ThenInclude(s => s.Staff)
                .Include(b => b.Service)
                    .ThenInclude(s => s.Category)
                .Include(b => b.Client)
                .Include(b => b.Payment)
                .Include(b => b.Review)
                .AsQueryable();

            if (from.HasValue)
            {
                var fromDate = from.Value.Date;
                query = query.Where(b => b.Date >= fromDate);
            }

            if (to.HasValue)
            {
                var toDate = to.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(b => b.Date <= toDate);
            }

            if (status.HasValue)
                query = query.Where(b => b.Status == status);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(b =>
                    b.Service.Name.Contains(search) ||
                    b.Client.FullName.Contains(search) ||
                    b.Service.Staff.FullName.Contains(search));

            if (!string.IsNullOrWhiteSpace(staffNameFilter))
                query = query.Where(b => b.Service.Staff.FullName.Contains(staffNameFilter));

            if (categoryIdFilter.HasValue)
                query = query.Where(b => b.Service.CategoryId == categoryIdFilter.Value);

            if (clientId.HasValue)
                query = query.Where(b => b.ClientId == clientId.Value);

            if (staffId.HasValue)
                query = query.Where(b => b.Service.StaffId == staffId.Value);

            return await query.CountAsync();
        }

        public async Task<bool> ExistsAsync(
            Guid staffId,
            DateTime date,
            TimeSpan time)
            => await _db.Bookings.AnyAsync(b =>
                b.Service.StaffId == staffId &&
                b.Date == date &&
                b.Time == time);

        public async Task<int> GetCountByStatusAsync(BookingStatus status, Guid? staffId = null)
        {
            var query = _db.Bookings.Where(b => b.Status == status);
            if (staffId.HasValue)
                query = query.Where(b => b.Service.StaffId == staffId.Value);

            return await query.CountAsync();
        }

        public async Task<double> GetTotalRevenueAsync(Guid? staffId = null)
        {
            var query = _db.Bookings.Where(b => b.Status == BookingStatus.Completed);
            if (staffId.HasValue)
                query = query.Where(b => b.Service.StaffId == staffId.Value);

            return await query.Include(b => b.Service).SumAsync(b => (double)b.Service.Price);
        }

        public async Task<double> GetTotalPlatformRevenueAsync()
        {
            return await _db.Bookings
                .Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Succeeded)
                .SumAsync(b => (double)b.Payment!.Amount);
        }

        public async Task SaveChangesAsync()
            => await _db.SaveChangesAsync();
    }
}

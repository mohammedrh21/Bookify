using Bookify.Application.DTO.Users;
using Bookify.Application.Interfaces.Staff;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly AppDbContext _db;

        public StaffRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Staff staff)
        {
            _db.Staffs.Add(staff);
            await _db.SaveChangesAsync();
        }

        public async Task<Staff?> GetByIdAsync(Guid id)
        {
            return await _db.Staffs.FindAsync(id);
        }

        public async Task UpdateAsync(Staff staff)
        {
            _db.Staffs.Update(staff);
            await _db.SaveChangesAsync();
        }

        public async Task<(IEnumerable<Staff> Items, int TotalCount)> GetStaffPaginatedAsync(int page, int pageSize)
        {
            var query = _db.Staffs
                .Include(s => s.Service)
                .AsNoTracking();

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        // ── Admin Staff Members ────────────────────────────────────────────────

        public async Task<(IEnumerable<AdminStaffDto> Items, int TotalCount)> GetAdminStaffPaginatedAsync(
            string? search, int page, int pageSize)
        {
            // Join Staffs with Identity Users to get email
            var query = from s in _db.Staffs
                            .Include(s => s.Service)
                        join u in _db.Users on s.Id equals u.Id
                        select new { Staff = s, User = u };

            // Apply search filter on FullName or Email
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(q =>
                    q.Staff.FullName.ToLower().Contains(lower) ||
                    (q.User.Email != null && q.User.Email.ToLower().Contains(lower)));
            }

            var total = await query.CountAsync();

            var paged = await query
                .OrderBy(q => q.Staff.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Gather aggregates for paged staff in one query
            var staffIds = paged.Select(p => p.Staff.Id).ToList();

            var bookingAggregates = await _db.Bookings
                .Where(b => b.Service != null && staffIds.Contains(b.Service.StaffId))
                .GroupBy(b => b.Service.StaffId)
                .Select(g => new
                {
                    StaffId = g.Key,
                    TotalBookings = g.Count(),
                    CompletedBookings = g.Count(b => b.Status == BookingStatus.Completed)
                })
                .ToListAsync();

            var result = paged.Select(item =>
            {
                var agg = bookingAggregates.FirstOrDefault(a => a.StaffId == item.Staff.Id);
                return new AdminStaffDto
                {
                    StaffId = item.Staff.Id,
                    FullName = item.Staff.FullName,
                    Email = item.User?.Email ?? string.Empty,
                    Phone = item.Staff.Phone ?? string.Empty,
                    ImagePath = item.Staff.ImagePath,
                    IsActive = item.Staff.IsActive,
                    ServiceId = item.Staff.Service?.Id,
                    ServiceName = item.Staff.Service?.Name ?? "No Service",
                    TotalBookings = agg?.TotalBookings ?? 0,
                    CompletedBookings = agg?.CompletedBookings ?? 0,
                    Rating = item.Staff.Service?.Rating ?? 0,
                    JoinedDate = null // Identity users don't store a joined date by default
                };
            }).ToList();

            return (result, total);
        }

        public async Task<AdminStaffDetailsDto?> GetAdminStaffDetailsAsync(Guid staffId)
        {
            // Fetch identity user for email
            var identityUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == staffId);
            var staff = await _db.Staffs
                .Include(s => s.Service)
                    .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == staffId);

            if (identityUser == null || staff == null) return null;

            // Fetch all bookings for this staff member's service
            var bookings = staff.Service == null
                ? new List<Booking>()
                : await _db.Bookings
                    .Include(b => b.Client)
                    .Include(b => b.Payment)
                    .Where(b => b.Service.StaffId == staffId)
                    .OrderByDescending(b => b.Date)
                    .ToListAsync();

            var completed = bookings.Where(b => b.Status == BookingStatus.Completed).ToList();
            var cancelled = bookings.Where(b => b.Status == BookingStatus.Cancelled).ToList();
            var upcoming  = bookings.Where(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved).ToList();

            var totalRevenue = completed
                .Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Succeeded)
                .Sum(b => b.Payment!.Amount);

            return new AdminStaffDetailsDto
            {
                StaffId = staffId,
                FullName = staff.FullName,
                Email = identityUser.Email ?? string.Empty,
                Phone = staff.Phone ?? string.Empty,
                ImagePath = staff.ImagePath,
                IsActive = staff.IsActive,

                ServiceId = staff.Service?.Id,
                ServiceName = staff.Service?.Name ?? string.Empty,
                ServiceDescription = staff.Service?.Description ?? string.Empty,
                ServicePrice = staff.Service?.Price ?? 0,
                CategoryName = staff.Service?.Category?.Name ?? string.Empty,
                Rating = staff.Service?.Rating ?? 0,
                ReviewCount = staff.Service?.ReviewCount ?? 0,

                TotalRevenue = totalRevenue,
                TotalBookings = bookings.Count,
                CompletedBookings = completed.Count,
                CancelledBookings = cancelled.Count,
                UpcomingBookings = upcoming.Count,

                Bookings = bookings.Select(b => new AdminStaffBookingDto
                {
                    Id = b.Id,
                    ClientName = b.Client?.FullName ?? "Unknown",
                    Date = b.Date,
                    Status = b.Status.ToString(),
                    Price = staff.Service?.Price ?? 0,
                    PaymentStatus = b.Payment?.Status.ToString() ?? "Unpaid"
                }).Take(5).ToList()
            };
        }
    }
}

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

        public async Task<(IEnumerable<Bookify.Application.DTO.Users.StaffClientDto> Items, int TotalCount)> GetStaffClientsPaginatedAsync(
            Guid staffId, string? search, DateTime? dateFilter, int page, int pageSize)
        {
            // 1. Get query of all bookings related to the staff member's service
            var targetBookings = _db.Bookings
                .Include(b => b.Service)
                .Where(b => b.Service.StaffId == staffId);

            // 2. Identify the unique clients that made these bookings
            var clientIds = targetBookings.Select(b => b.ClientId).Distinct();

            // 3. Construct the base query by joining Clients with Identity Users to get Email
            var query = from c in _db.Clients
                        join u in _db.Users on c.Id equals u.Id
                        where clientIds.Contains(c.Id)
                        select new { Client = c, User = u };

            // 4. Apply Filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(q => q.Client.FullName.ToLower().Contains(lowerSearch) || 
                                         (q.User.Email != null && q.User.Email.ToLower().Contains(lowerSearch)));
            }

            // We need to filter by booking date.
            // Client must have at least one booking with this staff on/after the specified date?
            // "date they booked" -> usually means the specific date.
            if (dateFilter.HasValue)
            {
                var filterDate = dateFilter.Value.Date;
                var clientsWithBookingsOnDate = targetBookings
                    .Where(b => b.Date.Date == filterDate)
                    .Select(b => b.ClientId)
                    .Distinct();

                query = query.Where(q => clientsWithBookingsOnDate.Contains(q.Client.Id));
            }

            var total = await query.CountAsync();

            // 5. Gather paginated records and compute aggregates
            var pagedClients = await query
                .OrderBy(q => q.Client.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<Bookify.Application.DTO.Users.StaffClientDto>();

            foreach (var item in pagedClients)
            {
                var cliBooks = await targetBookings
                    .Where(b => b.ClientId == item.Client.Id)
                    .OrderBy(b => b.Date)
                    .ToListAsync();

                result.Add(new Bookify.Application.DTO.Users.StaffClientDto
                {
                    ClientId = item.Client.Id,
                    FullName = item.Client.FullName,
                    Email = item.User?.Email ?? string.Empty,
                    Phone = item.Client.Phone ?? string.Empty,
                    ImagePath = item.Client.ImagePath,
                    TotalBookings = cliBooks.Count,
                    FirstBookingDate = cliBooks.FirstOrDefault()?.Date,
                    LastBookingDate = cliBooks.LastOrDefault()?.Date
                });
            }

            return (result, total);
        }

        public async Task<Bookify.Application.DTO.Users.StaffClientDetailsDto?> GetStaffClientDetailsAsync(Guid staffId, Guid clientId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == clientId);
            var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId);

            if (user == null || client == null) return null;

            var cliBooks = await _db.Bookings
                .Include(b => b.Service)
                .Include(b => b.Payment)
                .Where(b => b.Service.StaffId == staffId && b.ClientId == clientId)
                .OrderByDescending(b => b.Date)
                .ToListAsync();

            var completedBookings = cliBooks.Where(b => b.Status == Domain.Enums.BookingStatus.Completed).ToList();
            var cancelledBookings = cliBooks.Where(b => b.Status == Domain.Enums.BookingStatus.Cancelled).ToList();
            var upcomingBookings = cliBooks.Where(b => b.Status == Domain.Enums.BookingStatus.Pending || b.Status == Domain.Enums.BookingStatus.Approved).ToList();

            var totalRevenue = completedBookings
                .Where(b => b.Payment != null && b.Payment.Status == Domain.Enums.PaymentStatus.Succeeded)
                .Sum(b => b.Payment!.Amount);

            var dto = new Bookify.Application.DTO.Users.StaffClientDetailsDto
            {
                ClientId = clientId,
                FullName = client.FullName,
                Email = user.Email ?? string.Empty,
                Phone = client.Phone ?? string.Empty,
                ImagePath = client.ImagePath,
                TotalBookings = cliBooks.Count,
                CompletedBookings = completedBookings.Count,
                CancelledBookings = cancelledBookings.Count,
                UpcomingBookings = upcomingBookings.Count,
                TotalRevenue = totalRevenue,
                Bookings = cliBooks.Select(b => new Bookify.Application.DTO.Users.StaffClientBookingDto
                {
                    Id = b.Id,
                    Date = b.Date,
                    Status = b.Status.ToString(),
                    Price = b.Service.Price,
                    PaymentStatus = b.Payment?.Status.ToString() ?? "Unpaid"
                }).ToList()
            };

            return dto;
        }

        public async Task<(IEnumerable<Bookify.Application.DTO.Users.AdminClientDto> Items, int TotalCount)> GetAdminClientsPaginatedAsync(
            string? search, int page, int pageSize)
        {
            var query = from c in _db.Clients
                        join u in _db.Users on c.Id equals u.Id
                        select new { Client = c, User = u };

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(q => q.Client.FullName.ToLower().Contains(lowerSearch) || 
                                         (q.User.Email != null && q.User.Email.ToLower().Contains(lowerSearch)));
            }

            var total = await query.CountAsync();

            var pagedClients = await query
                .OrderBy(q => q.Client.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<Bookify.Application.DTO.Users.AdminClientDto>();

            var clientIds = pagedClients.Select(p => p.Client.Id).ToList();

            var clientBookings = await _db.Bookings
                .Where(b => clientIds.Contains(b.ClientId))
                .Select(b => new { b.ClientId, b.Date })
                .ToListAsync();

            foreach (var item in pagedClients)
            {
                var cliBooks = clientBookings
                    .Where(b => b.ClientId == item.Client.Id)
                    .OrderBy(b => b.Date)
                    .ToList();

                result.Add(new Bookify.Application.DTO.Users.AdminClientDto
                {
                    ClientId = item.Client.Id,
                    FullName = item.Client.FullName,
                    Email = item.User?.Email ?? string.Empty,
                    Phone = item.Client.Phone ?? string.Empty,
                    ImagePath = item.Client.ImagePath,
                    TotalBookings = cliBooks.Count,
                    LastBookingDate = cliBooks.LastOrDefault()?.Date,
                    IsActive = item.Client.IsActive
                });
            }

            return (result, total);
        }

        public async Task<Bookify.Application.DTO.Users.AdminClientDetailsDto?> GetAdminClientDetailsAsync(Guid clientId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == clientId);
            var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId);

            if (user == null || client == null) return null;

            var cliBooks = await _db.Bookings
                .Include(b => b.Service)
                .Include(b => b.Payment)
                .Where(b => b.ClientId == clientId)
                .OrderByDescending(b => b.Date)
                .ToListAsync();

            var completedBookings = cliBooks.Where(b => b.Status == Domain.Enums.BookingStatus.Completed).ToList();
            var cancelledBookings = cliBooks.Where(b => b.Status == Domain.Enums.BookingStatus.Cancelled).ToList();
            var upcomingBookings = cliBooks.Where(b => b.Status == Domain.Enums.BookingStatus.Pending || b.Status == Domain.Enums.BookingStatus.Approved).ToList();

            var totalRevenue = completedBookings
                .Where(b => b.Payment != null && b.Payment.Status == Domain.Enums.PaymentStatus.Succeeded)
                .Sum(b => b.Payment!.Amount);

            var dto = new Bookify.Application.DTO.Users.AdminClientDetailsDto
            {
                ClientId = clientId,
                FullName = client.FullName,
                Email = user.Email ?? string.Empty,
                Phone = client.Phone ?? string.Empty,
                ImagePath = client.ImagePath,
                IsActive = client.IsActive,
                TotalBookings = cliBooks.Count,
                CompletedBookings = completedBookings.Count,
                CancelledBookings = cancelledBookings.Count,
                UpcomingBookings = upcomingBookings.Count,
                TotalRevenue = totalRevenue,
                Bookings = cliBooks.Select(b => new Bookify.Application.DTO.Users.AdminClientBookingDto
                {
                    Id = b.Id,
                    ServiceName = b.Service.Name,
                    Date = b.Date,
                    Status = b.Status.ToString(),
                    Price = b.Service.Price,
                    PaymentStatus = b.Payment?.Status.ToString() ?? "Unpaid"
                }).ToList()
            };

            return dto;
        }
    }
}

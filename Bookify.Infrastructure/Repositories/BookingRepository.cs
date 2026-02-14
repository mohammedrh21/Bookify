using Bookify.Domain.Contracts.Booking;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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
            => await _db.Bookings.FirstOrDefaultAsync(b => b.Id == id);

        public async Task<IEnumerable<Booking>> GetByClientIdAsync(Guid clientId)
            => await _db.Bookings
                .Where(b => b.ClientId == clientId)
                .AsNoTracking()
                .ToListAsync();

        public async Task<IEnumerable<Booking>> GetByStaffIdAsync(Guid staffId)
            => await _db.Bookings
                .Where(b => b.Service.StaffId == staffId)
                .AsNoTracking()
                .ToListAsync();

        public async Task<IEnumerable<Booking>> GetAllAsync(
            DateTime? from,
            DateTime? to,
            BookingStatus? status)
        {
            var query = _db.Bookings.AsQueryable();

            if (from.HasValue)
                query = query.Where(b => b.Date >= from);

            if (to.HasValue)
                query = query.Where(b => b.Date <= to);

            if (status.HasValue)
                query = query.Where(b => b.Status == status);

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<bool> ExistsAsync(
            Guid staffId,
            DateTime date,
            TimeSpan time)
            => await _db.Bookings.AnyAsync(b =>
                b.Service.StaffId == staffId &&
                b.Date == date &&
                b.Time == time);

        public async Task SaveChangesAsync()
            => await _db.SaveChangesAsync();
    }
}

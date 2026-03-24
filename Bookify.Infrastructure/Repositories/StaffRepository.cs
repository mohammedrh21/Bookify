using Bookify.Application.Interfaces.Staff;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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

    }

}

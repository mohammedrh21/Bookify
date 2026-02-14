using Bookify.Application.Interfaces.Staff;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
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
    }

}

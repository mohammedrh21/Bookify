using Bookify.Domain.Contracts.ContactInfo;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Infrastructure.Repositories
{
    public class ContactInfoRepository : IContactInfoRepository
    {
        private readonly AppDbContext _db;
        public ContactInfoRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(ContactInfo info)
            => await _db.ContactInfo.AddAsync(info);

        public async Task<ContactInfo?> GetAsync()
            => await _db.ContactInfo.FirstOrDefaultAsync();

        public async Task UpdateAsync(ContactInfo info)
            => _db.ContactInfo.Update(info);

        public async Task SaveChangesAsync()
             => await _db.SaveChangesAsync();
    }
}

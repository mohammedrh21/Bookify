using Bookify.Domain.Contracts.FAQ;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Infrastructure.Repositories
{
    internal class FAQRepository : IFAQRepository
    {
        private readonly AppDbContext _db;
        public FAQRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(FAQ faq)
            => await _db.FAQs.AddAsync(faq);

        public async Task<IEnumerable<FAQ>> GetAllAsync()
            => await _db.FAQs.AsNoTracking().ToListAsync();

        public async Task<FAQ?> GetByIdAsync(Guid id)
            => await _db.FAQs.SingleOrDefaultAsync(x => x.Id == id);

        public async Task UpdateAsync(FAQ faq)
            => _db.FAQs.Update(faq);

        public void Delete(FAQ faq)
            => _db.FAQs.Remove(faq);

        public async Task SaveChangesAsync()
             => await _db.SaveChangesAsync();

    }
}

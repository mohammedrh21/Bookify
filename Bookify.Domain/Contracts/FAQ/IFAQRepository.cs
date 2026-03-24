using Bookify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Domain.Contracts.FAQ
{
    public interface IFAQRepository
    {
        Task<IEnumerable<Domain.Entities.FAQ>> GetAllAsync();
        Task<(IEnumerable<Domain.Entities.FAQ> Items, int TotalCount)> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null);
        Task<Domain.Entities.FAQ?> GetByIdAsync(Guid id);
        Task AddAsync(Domain.Entities.FAQ faq);
        Task UpdateAsync(Domain.Entities.FAQ faq);
        void Delete(Domain.Entities.FAQ faq);
        Task SaveChangesAsync();
    }
}

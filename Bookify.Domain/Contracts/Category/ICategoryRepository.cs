using Bookify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Domain.Contracts.Category
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Domain.Entities.Category>> GetAllAsync();
        Task AddAsync(Domain.Entities.Category Category);
        Task<Domain.Entities.Category?> GetByIdAsync(Guid id);
        Task UpdateAsync(Domain.Entities.Category Category);
        Task<bool> IsExists(string name);
        Task SaveChangesAsync();
    }
}

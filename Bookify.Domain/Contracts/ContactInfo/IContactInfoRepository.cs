using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Domain.Contracts.ContactInfo
{
    public interface IContactInfoRepository
    {
        Task AddAsync(Domain.Entities.ContactInfo info);
        Task<Domain.Entities.ContactInfo?> GetAsync();
        Task UpdateAsync(Domain.Entities.ContactInfo info);
        Task SaveChangesAsync();
    }
}



namespace Bookify.Application.Interfaces.Client
{
    public interface IClientRepository
    {
        Task AddAsync(Domain.Entities.Client client);
        //Task<Guid> GetByIdentityIdAsync(string identityId);
    }
}

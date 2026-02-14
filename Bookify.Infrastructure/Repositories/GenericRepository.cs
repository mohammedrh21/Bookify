using Bookify.Domain.Contracts;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Bookify.Infrastructure.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly AppDbContext _ctx;

        public GenericRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<TEntity?> GetByIdAsync(Guid id)
        {
            if (typeof(TEntity) == typeof(Domain.Entities.Service))
            {
                var product = await _ctx.Set<Domain.Entities.Service>()
                    .AsNoTracking()
                    .Include(p => p.Bookings)
                    .FirstOrDefaultAsync(p => p.Id == id);
                return product as TEntity;
            }
            return await _ctx.Set<TEntity>().FindAsync(id);
        }

        public async Task<int> AddAsync(TEntity entity)
        {
            _ctx.Set<TEntity>().Add(entity);

            return await _ctx.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var entity = await this.GetByIdAsync(id);

            if (entity == null)
            {
                return 0;
            }

            _ctx.Set<TEntity>().Remove(entity);
            return await _ctx.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(TEntity entity)
        {
            _ctx.Set<TEntity>().Update(entity);
            return await _ctx.SaveChangesAsync();
        }
    }
}

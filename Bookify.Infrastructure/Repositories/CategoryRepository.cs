using Bookify.Domain.Contracts.Category;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _db;
        public CategoryRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
            => await _db.Categories
                .Include(c => c.Services)
                .Where(x => x.IsActive)
                .AsNoTracking()
                .ToListAsync();

        public async Task AddAsync(Category category)
            => await _db.AddAsync(category);

        public async Task<Category?> GetByIdAsync(Guid id)
            => await _db.Categories
                .Include(c => c.Services)
                .FirstOrDefaultAsync(x => x.Id == id);

        public async Task<bool> IsExists(string name)
            => await _db.Categories.AnyAsync(x => x.Name == name);

        public async Task SaveChangesAsync()
            => await _db.SaveChangesAsync();

        public async Task UpdateAsync(Category Category)
        {
            var existingCategory = await _db.Categories.FindAsync(Category.Id);
            if (existingCategory != null)
            {
                existingCategory.Name = Category.Name;
                existingCategory.IsActive = Category.IsActive;
                _db.Categories.Update(existingCategory);
            }
        }
    }
}

using Blog.Data;
using Blog.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Repositories
{
    public class UserRepository : IRepository<User>
    {
        private readonly BlogDataContext _context;
        private readonly DbSet<User> _dbSet;
        public UserRepository(BlogDataContext context)
        {
            _context = context;
            _dbSet = context.Set<User>();
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var users = await _dbSet
                .AsNoTracking()
                .Include(u => u.Roles)
                .OrderBy(u => u.Id)
                .ToListAsync();

            return users;
        }

        public async Task<User> GetByIdAsync(int id)
        {
            var user = await _dbSet
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }
        public async Task<bool> AddAsync(User entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(User entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteAsync(User entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

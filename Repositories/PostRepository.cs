using Blog.Data;
using Blog.Models;
using Blog.ViewModels.Posts;
using Microsoft.EntityFrameworkCore;

namespace Blog.Repositories
{
    public class PostRepository : IRepository<Post>
    {
        private readonly BlogDataContext _context;
        private readonly DbSet<Post> _dbSet;
        public PostRepository(BlogDataContext context)
        {
            _context = context;
            _dbSet = context.Set<Post>();
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            var posts = await _dbSet
                .AsNoTracking()
                .Include(p => p.Author)
                .Include(c => c.Category)
                .OrderBy(p => p.Id)
                .ToListAsync();

            return posts;
        }

        public async Task<Post> GetByIdAsync(int id)
        {
            return await _dbSet.AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.Author)
                    .ThenInclude(a => a.Roles)
                    .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Post>> GetByCategoryAsync(string categoryName)
        {
            var posts = await _dbSet
                .AsNoTracking()
                .Include(p => p.Author)
                .Include(c => c.Category)
                .Where(p => p.Category.Slug == categoryName)
                .OrderByDescending(p => p.LastUpdateDate)
                .ToListAsync();

            return posts;
        }

        public async Task<bool> AddAsync(Post entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(Post entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteAsync(Post entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

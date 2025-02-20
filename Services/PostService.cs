using Blog.Models;
using Blog.Repositories;
using Blog.ViewModels.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Services
{
    public class PostService
    {
        private readonly PostRepository _repository;
        private readonly IMemoryCache _memoryCache;
        private readonly CategoryService _categoryService;
        public PostService(PostRepository repository, IMemoryCache memoryCache, CategoryService categoryService)
        {
            _repository = repository;
            _memoryCache = memoryCache;
            _categoryService = categoryService;
        }

        public async Task<IEnumerable<ListPostsViewModel>> ReadAllPostsAsync(int page, int pageSize)
        {
            var posts = await _memoryCache.GetOrCreateAsync("PostsCache", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return ReadPosts();
            });
            return posts.Skip(page * pageSize).Take(pageSize);
        }

        public async Task<IEnumerable<ListPostsViewModel>> ReadPosts()
        {
            var posts = await _repository.GetAllAsync();

            return posts
                .Select(p => new ListPostsViewModel()
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    LastUpdateDate = p.LastUpdateDate,
                    Category = p.Category.Name,
                    Author = $"{p.Author.Name} - ({p.Author.Email})"
                });
        }

        public async Task<Post> ReadPostByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Postagem inválida.");

            var post = await _repository.GetByIdAsync(id);

            if (post == null)
                throw new InvalidOperationException("Postagem não encontrada.");

            return post;
        }

        public async Task<IEnumerable<ListPostsViewModel>> ReadPostsByCategoryAsync(int page, int pageSize, string categoryName)
        {
            var posts = await _repository.GetAllAsync();

            return posts
                .Where(p => p.Category.Slug == categoryName)
                .Select(p => new ListPostsViewModel()
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    LastUpdateDate = p.LastUpdateDate,
                    Category = p.Category.Name,
                    Author = $"{p.Author.Name} - ({p.Author.Email})"
                }).Skip(page * pageSize).Take(pageSize);
        }

        public async Task<Post> CreatePostAsync(CreatePostViewModel model)
        {
            Post post = new Post()
            {
                Id = 0,
                Title = model.Title,
                Summary = model.Summary,
                Body = model.Body,
                Slug = model.Slug,
                AuthorId = model.AuthorId,
                CategoryId = model.CategoryId,
                CreateDate = DateTime.Now,
                LastUpdateDate = DateTime.Now,
            };
            var isCreated = await _repository.AddAsync(post);

            if (!isCreated)
                throw new DbUpdateException("Erro ao salvar a postagem.");

            return post;
        }

        public async Task<Post> UpdatePostAsync(int id, EditPostViewModel model)
        {
            if (id <= 0)
                throw new ArgumentException("Postagem inválida.");

            var post = await _repository.GetByIdAsync(id);
            if (post == null)
                throw new InvalidOperationException("Postagem não encontrada.");

            post.Title = string.IsNullOrWhiteSpace(model.Title) ? post.Title : model.Title;
            post.Summary = string.IsNullOrWhiteSpace(model.Summary) ? post.Summary : model.Summary;
            post.Body = string.IsNullOrWhiteSpace(model.Body) ? post.Body : model.Body;
            post.Slug = string.IsNullOrWhiteSpace(model.Slug) ? post.Slug : model.Slug.ToLower();
            post.LastUpdateDate = DateTime.Now;

            var category = await _categoryService.ReadCategoryByIdAsync(model.CategoryId);
            if (category == null)
                throw new InvalidOperationException("Categoria não encontrada.");

            post.CategoryId = model.CategoryId;

            var isUpdated = await _repository.UpdateAsync(post);
            if (!isUpdated)
                throw new DbUpdateException("Erro ao atualizar a postagem.");

            return post;
        }

        public async Task<Post> DeletePostAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Postagem inválida.");

            var post = await _repository.GetByIdAsync(id);
            if (post == null)
                throw new InvalidOperationException("Postagem não encontrada.");

            var isDeleted = await _repository.DeleteAsync(post);
            if (!isDeleted)
                throw new DbUpdateException("Erro ao remover a postagem.");

            return post;
        }
    }
}

using Blog.Models;
using Blog.Repositories;
using Blog.ViewModels;
using Blog.ViewModels.Categories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Services
{
    public class CategoryService
    {
        private readonly IRepository<Category> _repository;
        private readonly IMemoryCache _memoryCache;
        public CategoryService(IRepository<Category> repository, IMemoryCache memoryCache)
        {
            _repository = repository;
            _memoryCache = memoryCache;
        }
        public async Task<IEnumerable<Category>> ReadAllCategoriesAsync()
        {
            var categories = await _memoryCache.GetOrCreateAsync("CategoriesCache", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return ReadCategories();
            });
            return categories;
        }
        private async Task<IEnumerable<Category>> ReadCategories()
        {
            var categories = await _repository.GetAllAsync();

            return categories.OrderBy(c => c.Id);
        }
        public async Task<Category> ReadCategoryByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Categoria inválida");

            var category = await _repository.GetByIdAsync(id);
            if (category == null)
                throw new InvalidOperationException("Categoria não encontrada.");

            return category;
        }

        public async Task<Category> CreateCategoryAsync(EditorCategoryViewModel model)
        {
            Category category = new Category()
            {
                Id = 0,
                Name = model.Name,
                Slug = model.Slug.ToLower(),
                Posts = []
            };

            var isCreated = await _repository.AddAsync(category);
            if (!isCreated)
                throw new DbUpdateException("Erro ao salvar a categoria");

            return category;
        }

        public async Task<Category> UpdateCatetoryAsync(int id, EditorCategoryViewModel model)
        {
            if (id <= 0)
                throw new ArgumentException("Categoria inválida");

            var category = await _repository.GetByIdAsync(id);
            if (category == null)
                throw new InvalidOperationException("Categoria não encontrada.");

            category.Name = model.Name;
            category.Slug = model.Slug.ToLower();

            var isUpdated = await _repository.UpdateAsync(category);
            if (!isUpdated)
                throw new DbUpdateException("Erro ao atualizar a categoria");

            return category;
        }

        public async Task<Category> DeleteCatetoryAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Categoria inválida");

            var category = await _repository.GetByIdAsync(id);
            if (category == null)
                throw new InvalidOperationException("Categoria não encontrada.");

            var isDeleted = await _repository.DeleteAsync(category);
            if (!isDeleted)
                throw new DbUpdateException("Erro ao remover a categoria");

            return category;
        }
    }
}

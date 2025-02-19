using Blog.Models;
using Blog.Repositories;
using Blog.ViewModels.Tag;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Services
{
    public class TagService
    {
        private readonly IRepository<Tag> _repository;
        private readonly IMemoryCache _memoryCache;
        public TagService(IRepository<Tag> repository, IMemoryCache memoryCache)
        {
            _repository = repository;
            _memoryCache = memoryCache;
        }

        public async Task<IEnumerable<Tag>> ReadAllTagsAsync()
        {
            var tags = await _memoryCache.GetOrCreateAsync("TagsCache", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return ReadTags();
            });
            return tags;
        }

        public async Task<IEnumerable<Tag>> ReadTags()
        {
            var tags = await _repository.GetAllAsync();
            return tags.OrderBy(t => t.Id);
        }

        public async Task<Tag> ReadTagByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Tag inválida.");

            var tag = await _repository.GetByIdAsync(id);

            if (tag == null)
                throw new InvalidOperationException("Tag não encontrada.");

            return tag;
        }

        public async Task<Tag> CreateTagAsync(EditorTagViewModel model)
        {
            var tag = new Tag()
            {
                Id = 0,
                Name = model.Name,
                Slug = model.Slug.ToLower(),
                Posts = []
            };
            var isCreated = await _repository.AddAsync(tag);

            if(!isCreated)
                throw new DbUpdateException("Erro ao salvar a tag.");

            return tag;
        }

        public async Task<Tag> UpdateTagAsync(int id, EditorTagViewModel model)
        {
            if (id <= 0)
                throw new ArgumentException("Tag inválida.");

            var tag = await _repository.GetByIdAsync(id);
            if (tag == null)
                throw new InvalidOperationException("Tag não encontrada.");

            tag.Name = model.Name;
            tag.Slug = model.Slug.ToLower();

            var isUpdated = await _repository.UpdateAsync(tag);
            if (!isUpdated)
                throw new DbUpdateException("Erro ao atualizar a Tag.");

            return tag;
        }

        public async Task<Tag> DeleteTagAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Tag inválida.");

            var tag = await _repository.GetByIdAsync(id);
            if (tag == null)
                throw new InvalidOperationException("Tag não encontrada.");

            var isDeleted = await _repository.DeleteAsync(tag);
            if(!isDeleted)
                throw new DbUpdateException("Erro ao remover a tag.");

            return tag;
        }
    }
}

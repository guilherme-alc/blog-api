using Blog.Models;
using Blog.Repositories;
using Blog.ViewModels.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NuGet.Protocol.Core.Types;

namespace Blog.Services
{
    public class RoleService
    {
        private readonly IRepository<Role> _repository;
        private readonly IMemoryCache _memoryCache;
        public RoleService(IRepository<Role> repository, IMemoryCache memory)
        {
            _repository = repository;
            _memoryCache = memory;
        }
        public async Task<IEnumerable<Role>> ReadAllRolesAsync ()
        {
            var roles = await _memoryCache.GetOrCreateAsync("RolesCache", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return ReadRoles();
            });
            return roles;
        }
        public async Task<IEnumerable<Role>> ReadRoles ()
        {
            var roles = await _repository.GetAllAsync();
            return roles.OrderBy(r => r.Id);
        }
        
        public async Task<Role> ReadRoleById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Role inválida.");
            var role = await _repository.GetByIdAsync(id);
            if (role == null)
                throw new InvalidOperationException("Role não encontrada");
            return role;
        }

        public async Task<Role> CreateRoleAsync (EditorRoleViewModel model)
        {
            var role = new Role()
            {
                Name = model.Name,
                Slug = model.Slug.ToLower(),
                Users = []
            };

            var isCreated = await _repository.AddAsync(role);
            if (!isCreated)
                throw new DbUpdateException("Erro ao salvar a role.");

            return role;
        }

        public async Task<Role> UpdateRoleAsync (int id, EditorRoleViewModel model)
        {
            if (id <= 0)
                throw new ArgumentException("Role inválida.");
            var role = await _repository.GetByIdAsync(id);
            if (role == null)
                throw new InvalidOperationException("Role não encontrada.");
            role.Name = model.Name;
            role.Slug = model.Slug.ToLower();
            var isUpdated = await _repository.UpdateAsync(role);
            if (!isUpdated)
                throw new DbUpdateException("Erro ao atualizar role.");

            return role;
        }

        public async Task<Role> DeleteRoleAsync (int id)
        {
            if (id <= 0)
                throw new ArgumentException("Role inválida.");
            var role = await _repository.GetByIdAsync(id);
            if (role == null)
                throw new InvalidOperationException("Role não encontrada.");
            var isDeleted = await _repository.DeleteAsync(role);
            if (!isDeleted)
                throw new DbUpdateException("Erro ao remover role.");
            return role;
        }
    }
}

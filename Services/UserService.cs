using Blog.Models;
using Blog.Repositories;
using Blog.ViewModels.Users;

namespace Blog.Services
{
    public class UserService
    {
        private readonly IRepository<User> _repository;
        private readonly RoleService _roleService;
        public UserService(IRepository<User> repository, RoleService roleService)
        {
            _repository = repository;
            _roleService = roleService;
        }

        public async Task<IEnumerable<ListUserViewModel>> ReadAllUsersAsync(int page, int pageSize)
        {
            var users = await _repository.GetAllAsync();

            return users
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(u => new ListUserViewModel()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Slug = u.Slug,
                    CreateDate = u.CreateDate
                });
        }
        public async Task<User> ReadUserByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Usuário inválido.");

            var user = await _repository.GetByIdAsync(id);

            if (user == null)
                throw new InvalidOperationException("Usuário não encontrado.");

            return user;
        }

        public async Task<User> LinkRoleToUser(int userId, int roleId)
        {
            if (userId <= 0)
                throw new ArgumentException("Usuário inválido.");

            if(roleId <= 0)
                throw new ArgumentException("Usuário inválido.");

            var user = await _repository.GetByIdAsync(userId);

            if (user == null)
                throw new InvalidOperationException("Usuário não encontrado.");

            var role = await _roleService.ReadRoleById(roleId);

            if (role == null)
                throw new InvalidOperationException("Função não encontrada.");

            user.Roles.Add(role);
            await _repository.UpdateAsync(user);

            return user;
        }
    }
}

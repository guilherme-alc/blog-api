using Blog.Models;
using Blog.Repositories;
using Blog.ViewModels;
using Blog.ViewModels.Accounts;
using Blog.ViewModels.Users;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;

namespace Blog.Services
{
    public class UserService
    {
        private readonly UserRepository _repository;
        private readonly TokenService _tokenService;
        private readonly RoleService _roleService;
        public UserService(UserRepository repository, TokenService tokenService, RoleService roleService)
        {
            _repository = repository;
            _roleService = roleService;
            _tokenService = tokenService;
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

        public async Task<string> AuthenticateUser(LoginViewModel model)
        {
            var users = await _repository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null)
                throw new InvalidOperationException("Usuário ou senha inválidos.");

            if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
                throw new InvalidOperationException("Usuário ou senha inválidos.");

            var token = _tokenService.GenerateToken(user);

            return token;
        }

        public async Task<User> UpdateUserAsync (int userId, EditUserViewModel model)
        {
            var user = await _repository.GetByIdAsync(userId);

            user.Name = string.IsNullOrEmpty(model.Name) ? user.Name : model.Name;
            user.Email = string.IsNullOrEmpty(model.Email) ? user.Email : model.Email;
            user.Bio = string.IsNullOrEmpty(model.Bio) ? user.Bio : model.Bio;
            user.PasswordHash = string.IsNullOrEmpty(model.Password) ? user.PasswordHash : PasswordHasher.Hash(model.Password);

            await _repository.UpdateAsync(user);
            await _repository.SaveChangesAsync();

            return user;
        }

        public async Task<string> CreateUserAsync(RegisterUserViewModel model)
        {
            var user = new User()
            {
                Name = model.Name,
                Email = model.Email,
                Slug = model.Email.Replace("@", "-").Replace(".", "-"),
                CreateDate = DateTime.Now,
            };

            // Gera senha forte e codifica
            var password = PasswordGenerator.Generate(25, true, false);
            user.PasswordHash = PasswordHasher.Hash(password);

            var isCreated = await _repository.AddAsync(user);

            if (!isCreated)
                throw new DbUpdateException("Erro ao criar conta.");

            return password;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            var isDeleted = await _repository.DeleteAsync(user);

            return true;
        }

        public async Task<User> AddRoleToUser(int userId, int roleId)
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
            await _repository.SaveChangesAsync();

            return user;
        }

        public async Task<User> RemoveRoleToUser(int userId, int roleId)
        {
            if (userId <= 0)
                throw new ArgumentException("Usuário inválido.");

            if (roleId <= 0)
                throw new ArgumentException("Usuário inválido.");

            var user = await _repository.GetByIdAsync(userId);

            if (user == null)
                throw new InvalidOperationException("Usuário não encontrado.");

            var role = await _roleService.ReadRoleById(roleId);

            if (role == null)
                throw new InvalidOperationException("Função não encontrada.");

            user.Roles.Remove(role);
            await _repository.SaveChangesAsync();

            return user;
        }
    }
}

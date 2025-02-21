using Blog.Data;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _service;
        public UserController (UserService service)
        {
            _service = service;
        }
        [Authorize(Roles = "admin")]
        [HttpGet("v1/users")]
        public async Task<IActionResult> GetAsync([FromQuery] int page = 0, [FromQuery] int pageSize = 25)
        {
            try
            {
                var users = await _service.ReadAllUsersAsync(page, pageSize);
                var count = users.Count();

                return StatusCode(200, new ResultViewModel<dynamic>(new
                {
                    total = count,
                    page,
                    pageSize,
                    users
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        [Authorize(Roles ="admin")]
        [HttpGet("v1/users/{id:int}")]
        public async Task<IActionResult> GetDetailsAsync([FromRoute] int id)
        {
            try
            {
                var user = await _service.ReadUserByIdAsync(id);

                var userRoles = user.Roles.Select(r => r.Name).ToList();

                return StatusCode(200, new ResultViewModel<dynamic>(new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Slug,
                    user.CreateDate,
                    userRoles,
                    user.Bio
                }));
                
            }
            catch (ArgumentException ex)
            {
                return StatusCode(400, new ResultViewModel<string>(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(404, new ResultViewModel<string>(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost("v1/users/{userId:int}/role/{roleId:int}")]
        public async Task<IActionResult> LinkRoleToUser(
            [FromServices] BlogDataContext context,
            [FromRoute] int userId,
            [FromRoute] int roleId)
        {
            try
            {
                var user = await context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return StatusCode(404, new ResultViewModel<string>("Usuário não encontrado."));

                var role = await context.Roles.FindAsync(roleId);

                if (role == null)
                    return StatusCode(404, new ResultViewModel<string>("Perfil não encontrado."));

                user.Roles.Add(role);
                await context.SaveChangesAsync();

                var userRoles = user.Roles.Select(r => r.Name).ToList();

                return StatusCode(200, new ResultViewModel<dynamic>(new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    userRoles
                }));
            }
            catch (ArgumentException ex)
            {
                return StatusCode(400, new ResultViewModel<string>(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(404, new ResultViewModel<string>(ex.Message));

            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<string>("Erro ao vincular o usuário"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("v1/users/{userId:int}/role/{roleId:int}")]
        public async Task<IActionResult> DeleteRoleToUser(
            [FromServices] BlogDataContext context,
            [FromRoute] int userId,
            [FromRoute] int roleId)
        {
            try
            {
                var user = await context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return StatusCode(404, new ResultViewModel<string>("Usuário não encontrado."));

                var role = user.Roles.FirstOrDefault(r => r.Id == roleId);

                if (role == null)
                    return StatusCode(404, new ResultViewModel<string>("Perfil não encontrado."));

                user.Roles.Remove(role);
                await context.SaveChangesAsync();

                var userRoles = user.Roles.Select(r => r.Name).ToList();

                return StatusCode(200, new ResultViewModel<dynamic>(new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    userRoles
                }));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("05XE Falha interna no servidor."));
            }
        }
    }
}

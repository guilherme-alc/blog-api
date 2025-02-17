using Blog.Data;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        [Authorize(Roles = "admin")]
        [HttpGet("v1/users")]
        public async Task<IActionResult> GetAsync([FromServices] BlogDataContext context,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 25)
        {
            try
            {
                var count = await context.Users.CountAsync();
                var users = await context.Users
                    .AsNoTracking()
                    .Include(u => u.Roles)
                    .OrderBy(u => u.Id)
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .Select(u => new ListUserViewModel()
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                        Slug = u.Slug,
                        CreateDate = u.CreateDate
                    })
                    .ToListAsync();

                return StatusCode(200, new ResultViewModel<dynamic>(new
                {
                    total = count,
                    page,
                    pageSize,
                    users
                }));
            } 
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Post>>("05XE Falha interna no servidor."));
            }
        }

        [Authorize(Roles ="admin")]
        [HttpGet("v1/users/{id:int}")]
        public async Task<IActionResult> GetDetailsAsync(
            [FromServices] BlogDataContext context,
            [FromRoute] int id)
        {
            try
            {
                var user = await context.Users
                    .AsNoTracking()
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                    return StatusCode(404, new ResultViewModel<string>("Usuário não encontrado."));

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
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("05XE Falha interna no servidor."));
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
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("05XE Falha interna no servidor."));
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

using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Controllers
{
    [ApiController]
    public class RoleController : ControllerBase
    {
        [Authorize]
        [HttpGet("v1/roles")]
        public async Task<IActionResult> GetAsync(
            [FromServices] BlogDataContext context,
            [FromServices] IMemoryCache cache)
        {
            try
            {
                var roles = await cache.GetOrCreateAsync("RolesCache", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return GetRoles(context);
                });
                return Ok(new ResultViewModel<List<Role>>(roles));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<List<Role>>("05XE5 Falha interna no servidor."));
            }
        }

        private async Task<List<Role>> GetRoles(BlogDataContext context)
        {
            var roles = await context.Roles
                .AsNoTracking()
                .OrderBy(r => r.Id)
                .ToListAsync();
            return roles;
        }

        [Authorize]
        [HttpGet("v1/roles/{id:int}")]
        public async Task<IActionResult> GetByAsync(
            [FromServices] BlogDataContext context,
            [FromRoute] int id)
        {
            try
            {
                var role = await context.Roles.FindAsync(id);
                if (role == null)
                    return NotFound(new ResultViewModel<Role>("Perfil não encontrado."));

                return Ok(new ResultViewModel<Role>(role));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Role>("05X Falha interna no servidor."));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost("v1/roles")]
        public async Task<IActionResult> PostAsync([FromServices] BlogDataContext context,
            [FromBody] EditorRoleViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Role>(ModelState.GetErros()));
            try
            {
                var role = new Role()
                {
                    Name = model.Name,
                    Slug = model.Slug.ToLower(),
                    Users = []
                };
                await context.Roles.AddAsync(role);
                await context.SaveChangesAsync();

                return Created($"v1/roles/{role.Id}", new ResultViewModel<Role>(role));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Role>("05XE - Não foi possível incluir o perfil."));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Role>("05XE Falha interna no servidor."));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPut("v1/roles/{id:int}")]
        public async Task<IActionResult> PutAsync([FromServices] BlogDataContext context, [FromRoute] int id, [FromBody] EditorRoleViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Role>(ModelState.GetErros()));
            try
            {
                var role = await context.Roles.FindAsync(id);
                if (role == null)
                    return NotFound(new ResultViewModel<Role>("Perfil não encontrado."));

                role.Name = model.Name;
                role.Slug = model.Slug.ToLower();

                context.Roles.Update(role);
                await context.SaveChangesAsync();

                return Ok(new ResultViewModel<Role>(role));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Role>("05XE - Não foi possível alterar o perfil."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<Role>("05XE Falha interna no servidor."));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("v1/roles/{id:int}")]
        public async Task<IActionResult> DeleteAsync([FromServices] BlogDataContext context,
            [FromRoute] int id)
        {
            try
            {
                var role = await context.Roles.FindAsync(id);
                if(role == null)
                    return NotFound(new ResultViewModel<Role>("Perfil não encontrado."));

                context.Roles.Remove(role);
                await context.SaveChangesAsync();

                return Ok(new ResultViewModel<Role>(role));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Role>("05XE - Não foi possível deletar o perfil"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<Role>("05XE Falha interna no servidor"));
            }
        }
    }
}

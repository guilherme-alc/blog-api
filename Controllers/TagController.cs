using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Tag;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Controllers
{
    [ApiController]
    public class TagController : ControllerBase
    {
        [Authorize]
        [HttpGet("v1/tags")]
        public async Task<IActionResult> GetAsync(
            [FromServices] BlogDataContext context,
            [FromServices] IMemoryCache cache)
        {
            try
            {
                var tags = await cache.GetOrCreateAsync("TagsCache", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return GetTags(context);
                });
                return Ok(new ResultViewModel<List<Tag>>(tags));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("05XE Falha interna no servidor."));
            }
        }

        private async Task<List<Tag>> GetTags(BlogDataContext context)
        {
            var tags = await context.Tags
                .AsNoTracking()
                .OrderBy(t => t.Id)
                .ToListAsync();

            return tags;
        }

        [Authorize]
        [HttpGet("v1/tags/{id:int}")]
        public async Task<IActionResult> GetByAsync(
            [FromServices] BlogDataContext context,
            [FromRoute] int id)
        {
            try
            {
                var tag = await context.Tags.FindAsync(id);
                if (tag == null)
                    return NotFound(new ResultViewModel<Tag>("Tag não encontrada."));
                return Ok(new ResultViewModel<Tag>(tag));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Tag>("05X Falha interna no servidor."));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost("v1/tags")]
        public async Task<IActionResult> PostAsync(
            [FromServices] BlogDataContext context,
            [FromBody] EditorTagViewModel mdoel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResultViewModel<Tag>(ModelState.GetErros()));
            }
            try
            {
                var tag = new Tag()
                {
                    Name = mdoel.Name,
                    Slug = mdoel.Slug.ToLower(),
                    Posts = []
                };
                await context.Tags.AddAsync(tag);
                await context.SaveChangesAsync();

                return Created($"v1/tags/{tag.Id}", new ResultViewModel<Tag>(tag));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Tag>("05XE - Não foi possível incluir o perfil."));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Tag>("05XE Falha interna no servidor."));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPut("v1/tags/{id:int}")]
        public async Task<IActionResult> PutAsync(
            [FromServices] BlogDataContext context,
            [FromBody] EditorTagViewModel model,
            [FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Tag>(ModelState.GetErros()));
            try
            {
                var tag = await context.Tags.FindAsync(id);
                if (tag == null)
                    return NotFound(new ResultViewModel<Tag>("Tag não encontrada."));

                tag.Name = model.Name;
                tag.Slug = model.Slug.ToLower();

                context.Tags.Update(tag);
                await context.SaveChangesAsync();

                return Ok(new ResultViewModel<Tag>(tag));
            } 
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Tag>("05XE - Não foi possível alterar a tag."));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Tag>("05XE Falha interna no servidor."));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("v1/tags/{id:int}")]
        public async Task<IActionResult> DeleteAsync (
            [FromServices] BlogDataContext context,
            [FromRoute] int id)
        {
            try
            {
                var tag = await context.Tags.FindAsync(id);
                if (tag == null)
                    return NotFound(new ResultViewModel<Tag>("Tag não encontrada."));

                context.Tags.Remove(tag);
                await context.SaveChangesAsync();

                return Ok(new ResultViewModel<Tag>(tag));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Tag>("05XE - Não foi possível deletar a tag."));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Tag>("05XE Falha interna no servidor."));
            }
        }
    }
}

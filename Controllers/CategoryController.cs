using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Controllers
{
    [ApiController]
    public class CategoryController : ControllerBase
    {
        [Authorize]
        [HttpGet("v1/categories")]
        //[Route("v1/categories/{skip:int}/{take:int}")]
        public async Task<IActionResult> GetAsync (
            [FromServices] BlogDataContext context,
            [FromServices] IMemoryCache cache)
        {
            try
            {
                var categories = await cache.GetOrCreateAsync("CategoriesCache", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return GetCategories(context);
                });
                return Ok(new ResultViewModel<List<Category>>(categories));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("05XE5 Falha interna no servidor"));
            }
        }

        private async Task<List<Category>> GetCategories(BlogDataContext context)
        {
            var categories = await context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync();

            return categories;
        }

        [Authorize]
        [HttpGet("v1/categories/{id:int}")]
        public async Task<IActionResult> GetByIdAsync ([FromServices] BlogDataContext context, [FromRoute] int id)
        {
            try
            {
                var category = await context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound(new ResultViewModel<Category>("Categoria não encontrada."));

                return Ok(new ResultViewModel<Category>(category));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE6 Falha interna no servidor"));
            }

        }

        [Authorize(Roles = "admin")]
        [HttpPost("v1/categories")]
        public async Task<IActionResult> PostAsync ([FromServices] BlogDataContext context, [FromBody] EditorCategoryViewModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Category>(ModelState.GetErros()));
            try
            {
                Category category = new Category()
                {
                    Id = 0,
                    Name = model.Name,
                    Slug = model.Slug.ToLower(),
                    Posts = []
                };
                await context.Categories.AddAsync(category);
                await context.SaveChangesAsync();

                return Created($"v1/categories/{category.Id}", new ResultViewModel<Category>(category));
            } 
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE7 - Não foi possível incluir a categoria"));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE10 Falha interna no servidor"));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPut("v1/categories/{id:int}")]
        public async Task<IActionResult> PutAsync ([FromServices] BlogDataContext context, [FromRoute] int id, [FromBody] EditorCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Category>(ModelState.GetErros()));
            try
            {
                var category = await context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound(new ResultViewModel<Category>("Categoria não encontrada."));

                category.Name = model.Name;
                category.Slug = model.Slug.ToLower();

                context.Categories.Update(category);
                await context.SaveChangesAsync();

                return Ok(new ResultViewModel<Category>(category));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE8 - Não foi possível alterar a categoria"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE11 Falha interna no servidor"));
            }

        }

        [Authorize(Roles = "admin")]
        [HttpDelete("v1/categories/{id:int}")]
        public async Task<IActionResult> DeleteAsync ([FromServices] BlogDataContext context, [FromRoute] int id)
        {
            try
            {
                var category = await context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound(new ResultViewModel<Category>("Categoria não encontrada."));

                context.Categories.Remove(category);
                await context.SaveChangesAsync();

                return Ok(new ResultViewModel<Category>(category));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE9 - Não foi possível deletar a categoria"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE12 Falha interna no servidor"));
            }

        }
    }
}

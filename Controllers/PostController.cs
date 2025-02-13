using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
    [ApiController]
    public class PostController : ControllerBase
    {
        [Authorize]
        [HttpGet("v1/posts")]
        public async Task<IActionResult> GetAsync(
            [FromServices] BlogDataContext context,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 25)
        {
            try
            {
                var count = await context.Posts.AsNoTracking().CountAsync();
                var posts = await context
                    .Posts
                    .AsNoTracking()
                    .Include(p => p.Author)
                    .Include(p => p.Category)
                    .Select(p => new ListPostsViewModel()
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Slug = p.Slug,
                        LastUpdateDate = p.LastUpdateDate,
                        Category = p.Category.Name,
                        Author = $"{p.Author.Name} - ({p.Author.Email})"
                    })
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .OrderBy(p => p.Id)
                    .ToListAsync();
                return StatusCode(200, new ResultViewModel<dynamic>(new
                {
                    total = count,
                    page,
                    pageSize,
                    posts
                }));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Post>>("05XE21 Falha interna no servidor"));
            }
        }

        [Authorize]
        [HttpGet("v1/posts/{id:int}")]
        public async Task<IActionResult> GetDetailsAsync(
            [FromServices] BlogDataContext context,
            [FromRoute] int id)
        {
            try
            {
                var post = await context.Posts
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.Author)
                    .ThenInclude(a => a.Roles)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if(post == null)
                {
                    return NotFound(new ResultViewModel<string>("05XE22 Post não encontrado"));
                }

                return Ok(new ResultViewModel<Post>(post));

            }catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("05XE20 Falha interna no servidor"));
            }
        }

        [Authorize]
        [HttpGet("v1/posts/category/{category}")]
        public async Task<IActionResult> GetByGategoryAsync(
            [FromServices] BlogDataContext context,
            [FromRoute] string category,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 25)
        {
            try
            {
                var count = await context.Posts.AsNoTracking().CountAsync();
                var posts = context.Posts
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.Author)
                    .Where(p => p.Category.Slug == category)
                    .Select(p => new ListPostsViewModel()
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Slug = p.Slug,
                        LastUpdateDate = p.LastUpdateDate,
                        Category = p.Category.Name,
                        Author = $"{p.Author.Name} ({p.Author.Email})"
                    })
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .OrderByDescending(p => p.LastUpdateDate)
                    .ToListAsync();

                return Ok(new ResultViewModel<dynamic>(new
                {
                    total = count,
                    page,
                    pageSize,
                    posts
                }));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Post>>("05XE21 Falha interna no servidor"));
            }
        }
        
        [Authorize(Roles = "admin,author")]
        [HttpPost("v1/posts")]
        public async Task<IActionResult> PostAsync(
            [FromServices] BlogDataContext context,
            [FromBody] CreatePostViewModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Post>(ModelState.GetErros()));
            try
            {
                Post post = new Post()
                {
                    Id = 0,
                    Title = model.Title,
                    Summary = model.Summary,
                    Body = model.Body,
                    Slug = model.Slug,
                    AuthorId = model.AuthorId,
                    CategoryId = model.CategoryId,
                    CreateDate = DateTime.Now,
                    LastUpdateDate = DateTime.Now,
                };

                await context.Posts.AddAsync(post);
                await context.SaveChangesAsync();

                return Created($"v1/posts/{post.Id}", new ResultViewModel<Post>(post));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE - Não foi possível incluir o post"));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE Falha interna no servidor"));
            }
        }

        [Authorize(Roles = "admin,author")]
        [HttpPut("v1/posts/{id:int}")]
        public async Task<IActionResult> PutAsync([FromServices] BlogDataContext context,
            [FromBody] EditPostViewModel model,
            [FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Post>(ModelState.GetErros()));
            try
            {
                var post = await context.Posts.FindAsync(id);
                if (post == null)
                    return NotFound(new ResultViewModel<Post>("Postagem não encontrada."));

                post.Title = string.IsNullOrWhiteSpace(model.Title) ? post.Title : model.Title;
                post.Summary = string.IsNullOrWhiteSpace(model.Summary) ? post.Summary : model.Summary;
                post.Body = string.IsNullOrWhiteSpace(model.Body) ? post.Body : model.Body;
                post.Slug = string.IsNullOrWhiteSpace(model.Slug) ? post.Slug : model.Slug.ToLower();
                post.LastUpdateDate = DateTime.Now;

                var categoryExists = await context.Categories.AnyAsync(c => c.Id == model.CategoryId);
                if (!categoryExists)
                    return BadRequest(new ResultViewModel<Post>("Categoria inválida ou inexistente."));
                post.CategoryId = model.CategoryId;


                context.Posts.Update(post);
                await context.SaveChangesAsync();

                return Ok(new ResultViewModel<Post>(post));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Post>("05XE - Não foi possível alterar a postagem"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<Post>("05XE Falha interna no servidor"));
            }

        }

        [Authorize(Roles = "admin,author")]
        [HttpDelete("v1/posts/{id:int}")]
        public async Task<IActionResult> DeleteAsync(
            [FromServices] BlogDataContext context, 
            [FromRoute] int id)
        {
            try
            {
                var post = context.Posts.Find(id);
                if (post == null)
                    return NotFound(new ResultViewModel<Post>("Postagem não encontrada."));
                context.Posts.Remove(post);
                await context.SaveChangesAsync();

                return Ok(new ResultViewModel<Post>(post));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Post>("05XE - Não foi possível deletar a postagem."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<Post>("05XE Falha interna no servidor"));
            }
        }
    }
}
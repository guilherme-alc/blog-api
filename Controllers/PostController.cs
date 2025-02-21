using Azure;
using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
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
        private readonly PostService _service;
        public PostController(PostService service)
        {
            _service = service;
        }
        [Authorize]
        [HttpGet("v1/posts")]
        public async Task<IActionResult> GetAsync([FromQuery] int page = 0, [FromQuery] int pageSize = 25)
        {
            try
            {
                var posts = await _service.ReadAllPostsAsync(page, pageSize);
                var count = posts.ToList().Count();

                return StatusCode(200, new ResultViewModel<dynamic>(new
                {
                    total = count,
                    page,
                    pageSize,
                    posts
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        [Authorize]
        [HttpGet("v1/posts/{id:int}")]
        public async Task<IActionResult> GetDetailsAsync([FromRoute] int id)
        {
            try
            {
                var post = await _service.ReadPostByIdAsync(id);

                return StatusCode(200, new ResultViewModel<Post>(post));

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

        [Authorize]
        [HttpGet("v1/posts/category/{categoryName}")]
        public async Task<IActionResult> GetByGategoryAsync(
            [FromRoute] string categoryName,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 25)
        {
            try
            {
                var posts = await _service.ReadPostsByCategoryAsync(page, pageSize, categoryName);
                var count = posts.ToList().Count();

                return Ok(new ResultViewModel<dynamic>(new
                {
                    total = count,
                    page,
                    pageSize,
                    posts
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
        
        [Authorize(Roles = "admin,author")]
        [HttpPost("v1/posts")]
        public async Task<IActionResult> PostAsync([FromBody] CreatePostViewModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Post>(ModelState.GetErros()));
            try
            {
                var post = await _service.CreatePostAsync(model);

                return Created($"v1/posts/{post.Id}", new ResultViewModel<Post>(post));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<string>("Erro ao salvar a função"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        [Authorize(Roles = "admin,author")]
        [HttpPut("v1/posts/{id:int}")]
        public async Task<IActionResult> PutAsync([FromBody] EditPostViewModel model,
            [FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Post>(ModelState.GetErros()));
            try
            {
                var post = await _service.UpdatePostAsync(id, model);

                return StatusCode(200, new ResultViewModel<Post>(post));
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
                return StatusCode(500, new ResultViewModel<string>("Erro ao atualizar a função"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }

        }

        [Authorize(Roles = "admin,author")]
        [HttpDelete("v1/posts/{id:int}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            try
            {
                var post = await _service.DeletePostAsync(id);

                return StatusCode(200, new ResultViewModel<Post>(post));
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
                return StatusCode(500, new ResultViewModel<string>("Erro ao deletar a função"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }
    }
}
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Blog.ViewModels.Tag;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly TagService _service;
        public TagController(TagService service)
        {
            _service = service;
        }
        [Authorize]
        [HttpGet("v1/tags")]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                var tags = await _service.ReadAllTagsAsync();
                return StatusCode(200, new ResultViewModel<IEnumerable<Tag>>(tags));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        [Authorize]
        [HttpGet("v1/tags/{id:int}")]
        public async Task<IActionResult> GetByAsync(
            [FromRoute] int id)
        {
            try
            {
                var tag = await _service.ReadTagByIdAsync(id);

                return StatusCode(200, new ResultViewModel<Tag>(tag));
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
        [HttpPost("v1/tags")]
        public async Task<IActionResult> PostAsync(
            [FromBody] EditorTagViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, new ResultViewModel<List<string>>(ModelState.GetErros()));
            }
            try
            {
                var tag = await _service.CreateTagAsync(model);

                return Created($"v1/tags/{tag.Id}", new ResultViewModel<Tag>(tag));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<string>("Erro ao salvar a categoria"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPut("v1/tags/{id:int}")]
        public async Task<IActionResult> PutAsync(
            [FromBody] EditorTagViewModel model,
            [FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<List<string>>(ModelState.GetErros()));
            try
            {
                var tag = await _service.UpdateTagAsync(id, model);

                return StatusCode(200, new ResultViewModel<Tag>(tag));
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
                return StatusCode(500, new ResultViewModel<string>("Erro ao atualizar a categoria"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("v1/tags/{id:int}")]
        public async Task<IActionResult> DeleteAsync (
            [FromRoute] int id)
        {
            try
            {
                var tag = await _service.DeleteTagAsync(id);

                return StatusCode(200, new ResultViewModel<Tag>(tag));
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
                return StatusCode(500, new ResultViewModel<string>("Erro ao deletar a categoria"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }
    }
}

using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Blog.ViewModels.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.Intrinsics.Arm;

namespace Blog.Controllers
{
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _service;
        public CategoryController(CategoryService service)
        {
            _service = service;
        }
        [Authorize]
        [HttpGet("v1/categories")]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                var categories = await _service.ReadAllCategoriesAsync();
                return Ok(new ResultViewModel<IEnumerable<Category>>(categories));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        [Authorize]
        [HttpGet("v1/categories/{id:int}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
        {
            try
            {
                var category = await _service.ReadCategoryByIdAsync(id);
                return Ok(new ResultViewModel<Category>(category));
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
        [HttpPost("v1/categories")]
        public async Task<IActionResult> PostAsync ([FromBody] EditorCategoryViewModel model)
        {
            if(!ModelState.IsValid)
                return StatusCode(400, new ResultViewModel<List<string>>(ModelState.GetErros()));
            try
            {
                var category = await _service.CreateCategoryAsync(model);

                return Created($"v1/categories/{category.Id}", new ResultViewModel<Category>(category));
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
        [HttpPut("v1/categories/{id:int}")]
        public async Task<IActionResult> PutAsync ([FromRoute] int id, [FromBody] EditorCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<List<string>>(ModelState.GetErros()));
            try
            {
                var category = await _service.UpdateCatetoryAsync(id, model);

                return Ok(new ResultViewModel<Category>(category));
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
        [HttpDelete("v1/categories/{id:int}")]
        public async Task<IActionResult> DeleteAsync ([FromRoute] int id)
        {
            try
            {
                var category = await _service.DeleteCatetoryAsync(id);

                return Ok(new ResultViewModel<Category>(category));
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

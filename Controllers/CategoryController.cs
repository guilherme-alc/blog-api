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

        /// <summary>
        /// Obter todas as categorias
        /// </summary>
        /// <returns>Coleção de categorias</returns>
        /// <response code="200">Sucesso</response>
        /// <response code="401">Não autenticado</response>

        [ProducesResponseType(typeof(ResultViewModel<IEnumerable<Category>>), StatusCodes.Status200OK)]
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

        /// <summary>
        /// Obter uma categoria
        /// </summary>
        /// <param name="id">Identificador da categoria</param>
        /// <returns>Dados da categoria</returns>
        /// <response code="200">Sucesso</response>
        /// <response code="400">Identificador inválido</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="404">Categoria não encontrada</response>
        /// <response code="500">Falha no servidor</response>

        [ProducesResponseType(typeof(ResultViewModel<Category>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Cadastrar uma categoria
        /// </summary>
        /// <param name="model">Dados da categoria</param>
        /// <returns>Categoria recém criada</returns>
        /// <response code="201">Sucesso</response>
        /// <response code="400">Categoria inválida</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="403">Sem permissão</response>
        /// <response code="500">Falha no servidor</response>

        [ProducesResponseType(typeof(ResultViewModel<Category>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
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


        /// <summary>
        /// Atualiza uma categoria
        /// </summary>
        /// <param name="id">Identificador da categoria</param>
        /// <param name="model">Dados da categoria</param>
        /// <returns>Categoria atualizada</returns>
        /// <response code="204">Sucesso</response>
        /// <response code="400">Categoria inválida</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="403">Sem permissão</response>
        /// <response code="404">Categoria não encontrada</response>
        /// <response code="500">Falha no servidor</response>

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "admin")]
        [HttpPut("v1/categories/{id:int}")]
        public async Task<IActionResult> PutAsync ([FromRoute] int id, [FromBody] EditorCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<List<string>>(ModelState.GetErros()));
            try
            {
                var category = await _service.UpdateCatetoryAsync(id, model);

                return StatusCode(204);
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

        /// <summary>
        /// Remove uma categoria
        /// </summary>
        /// <param name="id">Identificador da categoria</param>
        /// <returns>Categoria exclúida</returns>
        /// <response code="204">Sucesso</response>
        /// <response code="400">Identificador inválido</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="403">Sem permissão</response>
        /// <response code="404">Categoria não encontrada</response>

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "admin")]
        [HttpDelete("v1/categories/{id:int}")]
        public async Task<IActionResult> DeleteAsync ([FromRoute] int id)
        {
            try
            {
                var category = await _service.DeleteCatetoryAsync(id);

                return StatusCode(204);
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

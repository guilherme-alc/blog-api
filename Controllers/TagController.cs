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

        /// <summary>
        /// Obter todas as tags
        /// </summary>
        /// <returns>Coleção de tags</returns>
        /// <response code="200">Sucesso</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="500">Falha no servidor</response>
        [ProducesResponseType(typeof(ResultViewModel<IEnumerable<Tag>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Obter detalhes de uma tag
        /// </summary>
        /// <param name="id">Identificador da tag</param>
        /// <returns>Dados da tag</returns>
        [ProducesResponseType(typeof(ResultViewModel<Tag>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Cadastrar uma tag
        /// </summary>
        /// <param name="model">Dados da tag</param>
        /// <returns>Tag recém criada</returns>
        /// <response code="201">Sucesso</response>
        /// <response code="400">Tag inválida</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="403">Sem permissão</response>
        /// <response code="500">Falha no servidor</response>
        [ProducesResponseType(typeof(ResultViewModel<Tag>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
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
                return StatusCode(500, new ResultViewModel<string>("Erro ao salvar a tag"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        /// <summary>
        /// Atualiza uma tag
        /// </summary>
        /// <param name="id">Identificador da tag</param>
        /// <param name="model">Dados da tag</param>
        /// <returns>Tag atualizada</returns>
        /// <response code="204">Sucesso</response>
        /// <response code="400">Tag inválida</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="403">Sem permissão</response>
        /// <response code="404">Tag não encontrada</response>
        /// <response code="500">Falha no servidor</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
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
                return StatusCode(500, new ResultViewModel<string>("Erro ao atualizar a tag"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        /// <summary>
        /// Remove uma tag
        /// </summary>
        /// <param name="id">Identificador da tag</param>
        /// <returns>Tag exclúida</returns>
        /// <response code="204">Sucesso</response>
        /// <response code="400">Identificador inválido</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="403">Sem permissão</response>
        /// <response code="404">Tag não encontrada</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
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
                return StatusCode(500, new ResultViewModel<string>("Erro ao deletar a tag"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }
    }
}

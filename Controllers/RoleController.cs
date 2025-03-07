using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Blog.ViewModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly RoleService _service;
        public RoleController (RoleService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obter todas as funções
        /// </summary>
        /// <returns>Coleção de tags</returns>
        /// <response code="200">Sucesso</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="500">Falha no servidor</response>
        [ProducesResponseType(typeof(ResultViewModel<IEnumerable<Role>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
        [Authorize]
        [HttpGet("v1/roles")]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                var roles = await _service.ReadAllRolesAsync();
                return StatusCode(200, new ResultViewModel<IEnumerable<Role>>(roles));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        /// <summary>
        /// Obter detalhes de uma função
        /// </summary>
        /// <param name="id">Identificador da função</param>
        /// <returns>Dados da função</returns>
        [ProducesResponseType(typeof(ResultViewModel<Role>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
        [Authorize]
        [HttpGet("v1/roles/{id:int}")]
        public async Task<IActionResult> GetByAsync([FromRoute] int id)
        {
            try
            {
                var role = await _service.ReadRoleById(id);

                return StatusCode(200, new ResultViewModel<Role>(role));
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
        /// Cadastrar uma função
        /// </summary>
        /// <param name="model">Dados da função</param>
        /// <returns>Função recém criada</returns>
        /// <response code="201">Sucesso</response>
        /// <response code="400">Função inválida</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="403">Sem permissão</response>
        /// <response code="500">Falha no servidor</response>
        [ProducesResponseType(typeof(ResultViewModel<Role>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "admin")]
        [HttpPost("v1/roles")]
        public async Task<IActionResult> PostAsync([FromBody] EditorRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, new ResultViewModel<List<string>>(ModelState.GetErros()));
            }
            try
            {
                var role = await _service.CreateRoleAsync(model);

                return Created($"v1/roles/{role.Id}", new ResultViewModel<Role>(role));
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

        /// <summary>
        /// Atualiza uma função
        /// </summary>
        /// <param name="id">Identificador da função</param>
        /// <param name="model">Dados da função</param>
        /// <returns>Função atualizada</returns>
        /// <response code="204">Sucesso</response>
        /// <response code="400">Função inválida</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="403">Sem permissão</response>
        /// <response code="404">Função não encontrada</response>
        /// <response code="500">Falha no servidor</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "admin")]
        [HttpPut("v1/roles/{id:int}")]
        public async Task<IActionResult> PutAsync([FromRoute] int id, [FromBody] EditorRoleViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<List<string>>(ModelState.GetErros()));
            try
            {
                var role = await _service.UpdateRoleAsync(id, model);

                return StatusCode(200, new ResultViewModel<Role>(role));
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

        /// <summary>
        /// Remove uma função
        /// </summary>
        /// <param name="id">Identificador da função</param>
        /// <returns>Função exclúida</returns>
        /// <response code="204">Sucesso</response>
        /// <response code="400">Identificador inválido</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="403">Sem permissão</response>
        /// <response code="404">Função não encontrada</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "admin")]
        [HttpDelete("v1/roles/{id:int}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            try
            {
                var role = await _service.DeleteRoleAsync(id);

                return StatusCode(200, new ResultViewModel<Role>(role));
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

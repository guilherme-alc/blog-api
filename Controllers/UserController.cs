using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _service;
        public UserController (UserService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obter todos os usuários
        /// </summary>
        /// <returns>Coleção de usuários</returns>
        /// <response code="200">Sucesso</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="500">Falha no servidor</response>
        [ProducesResponseType(typeof(ResultViewModel<IEnumerable<User>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "admin")]
        [HttpGet("v1/users")]
        public async Task<IActionResult> GetAsync([FromQuery] int page = 0, [FromQuery] int pageSize = 25)
        {
            try
            {
                var users = await _service.ReadAllUsersAsync(page, pageSize);
                var count = users.Count();

                return StatusCode(200, new ResultViewModel<dynamic>(new
                {
                    total = count,
                    page,
                    pageSize,
                    users
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        /// <summary>
        /// Obter detalhes de um usuário
        /// </summary>
        /// <param name="id">Identificador do usuário</param>
        /// <returns>Dados do usuário</returns>
        [ProducesResponseType(typeof(ResultViewModel<User>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles ="admin")]
        [HttpGet("v1/users/{id:int}")]
        public async Task<IActionResult> GetDetailsAsync([FromRoute] int id)
        {
            try
            {
                var user = await _service.ReadUserByIdAsync(id);

                var userRoles = user.Roles.Select(r => r.Name).ToList();

                return StatusCode(200, new ResultViewModel<dynamic>(new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Slug,
                    user.CreateDate,
                    userRoles,
                    user.Bio
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

        /// <summary>
        /// Vincular um usuário a uma função
        /// </summary>
        /// <response code="204">Sucesso</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="404">Não autorizado</response>
        /// <response code="500">Falha ao vincular usuário</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "admin")]
        [HttpPost("v1/users/{userId:int}/role/{roleId:int}")]
        public async Task<IActionResult> LinkRoleToUser([FromRoute] int userId, [FromRoute] int roleId)
        {
            try
            {
                var user = await _service.AddRoleToUser(userId, roleId);

                var userRoles = user.Roles.Select(r => r.Name).ToList();

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
                return StatusCode(500, new ResultViewModel<string>("Erro ao vincular o usuário"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        /// <summary>
        /// Desvincular um usuário a uma função
        /// </summary>
        /// <response code="204">Sucesso</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="404">Não autorizado</response>
        /// <response code="500">Falha ao desvincular usuário</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "admin")]
        [HttpDelete("v1/users/{userId:int}/role/{roleId:int}")]
        public async Task<IActionResult> DeleteRoleToUser([FromRoute] int userId, [FromRoute] int roleId)
        {
            try
            {
                var user = await _service.RemoveRoleToUser(userId, roleId);

                var userRoles = user.Roles.Select(r => r.Name).ToList();

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
                return StatusCode(500, new ResultViewModel<string>("Erro ao vincular o usuário"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }
    }
}

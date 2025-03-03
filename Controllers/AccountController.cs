using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Blog.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Blog.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        public AccountController(UserService service)
        {
            _service = service;
        }
        private readonly UserService _service;

        /// <summary>
        /// Cria nova conta
        /// </summary>
        /// <param name="model">Dados iniciais do usuário</param>
        /// <returns>E-mail e senha da conta criada</returns>
        /// <response code="201">Sucesso</response>
        /// <response code="400">Dado(s) inválido(s)</response>
        /// <response code="500">Falha no servidor</response>

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
        [HttpPost("v1/accounts")]
        public async Task<IActionResult> Register ([FromBody] RegisterUserViewModel model)
        {
            if(!ModelState.IsValid)
                return StatusCode(400, new ResultViewModel<string>(ModelState.GetErros()));

            try
            {
                var password = await _service.CreateUserAsync(model);

                return StatusCode(201, new ResultViewModel<dynamic>(new
                {
                    user = model.Email,
                    password
                }));
            } catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<string>(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        /// <summary>
        /// Autentica usuário
        /// </summary>
        /// <param name="model">Dados de login da conta</param>
        /// <returns>Token de autenticação</returns>
        /// <response code="200">Sucesso</response>
        /// <response code="400">Credencial inválida</response>
        /// <response code="500">Falha no servidor</response>

        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
        [HttpPost("v1/accounts/login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if(!ModelState.IsValid)
                return StatusCode(400, new ResultViewModel<string>(ModelState.GetErros()));
            try
            {
                var token = await _service.AuthenticateUser(model);
                return StatusCode(200, new ResultViewModel<string>(token, null));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(400, new ResultViewModel<string>(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        /// <summary>
        /// Atualiza dados da conta
        /// </summary>
        /// <param name="model">Dados do usuário</param>
        /// <returns>Usuário atualizado</returns>
        /// <response code="200">Sucesso</response>
        /// <response code="500">Falha no servidor</response>

        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
        [Authorize]
        [HttpPatch("v1/accounts")]
        public async Task<IActionResult> Edit ([FromBody] EditUserViewModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest(new ResultViewModel<User>(ModelState.GetErros()));
            try 
            {
                var userId = GetUserIdFromToken();
                var user = await _service.UpdateUserAsync(userId, model);

                return StatusCode(200, new ResultViewModel<dynamic>(new { user.Name, user.Email, user.Bio, model.Password }));
            } 
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        /// <summary>
        /// Deleta conta
        /// </summary>
        /// <returns>Confirmação de conta deletada</returns>
        /// <response code="200">Sucesso</response>
        /// <response code="500">Falha no servidor</response>

        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
        [Authorize]
        [HttpDelete("v1/accounts")]
        public async Task<IActionResult> Delete ()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var user = await _service.DeleteUserAsync(userId);

                return Ok(new ResultViewModel<string>("Conta deletada com sucesso!"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        private int GetUserIdFromToken()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid");

            return userIdClaim != null ? int.Parse(userIdClaim.Value) : -1;
        }
    }
}

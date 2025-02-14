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
        [HttpPost("v1/accounts")]
        public async Task<IActionResult> Register ([FromServices] BlogDataContext context, 
            [FromBody] RegisterUserViewModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErros()));

            var user = new User()
            {
                Name = model.Name,
                Email = model.Email,
                Slug = model.Email.Replace("@", "-").Replace(".", "-")
            };

            // Gera senha forte e codifica
            var password = PasswordGenerator.Generate(25, true, false);
            user.PasswordHash = PasswordHasher.Hash(password);

            try
            {
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                return Ok(new ResultViewModel<dynamic>(new
                {
                    user = user.Email,
                    password = password
                }));
            } catch (DbUpdateException ex)
            {
                return StatusCode(400, new ResultViewModel<string>("05X20 - Este E-mail já está cadastrado."));
            } catch
            {
                return StatusCode(500, new ResultViewModel<string>("05X19 - Falha interna no servidor."));
            }
        }

        [HttpPost("v1/accounts/login")]
        public async Task<IActionResult> Login([FromServices] BlogDataContext context,
            [FromServices] TokenService tokenService,
            [FromBody] LoginViewModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErros()));

            var user = await context.Users
                .AsNoTracking()
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos."));

            if(!PasswordHasher.Verify(user.PasswordHash, model.Password))
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos."));

            try
            {
                var token = tokenService.GenerateToken(user);
                return Ok(new ResultViewModel<string>(token, null));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("05X18 - Falha interna no servidor."));
            }
        }

        [Authorize]
        [HttpPatch("v1/accounts")]
        public async Task<IActionResult> Edit (
            [FromServices] BlogDataContext context,
            [FromBody] EditUserViewModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest(new ResultViewModel<User>(ModelState.GetErros()));

            var userId = GetUserIdFromToken();
            var user = await context.Users.FindAsync(userId);

            if (user == null)
                return NotFound("Usuário não encontrado.");

            user.Name = string.IsNullOrEmpty(model.Name) ? user.Name : model.Name;
            user.Email = string.IsNullOrEmpty(model.Email) ? user.Email : model.Email;
            user.Bio = string.IsNullOrEmpty(model.Bio) ? user.Bio : model.Bio;
            user.PasswordHash = string.IsNullOrEmpty(model.Password) ? user.PasswordHash : PasswordHasher.Hash(model.Password);

            context.Update(user);
            await context.SaveChangesAsync();

            return Ok(new ResultViewModel<EditUserViewModel>(model));
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

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

        /// <summary>
        /// Obter todas as postagens
        /// </summary>
        /// <param name="page">Página da busca</param>
        /// <param name="pageSize">Quantidade de registros por página</param>
        /// <returns>Coleção de postagens</returns>
        /// <response code="200">Sucesso</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="500">Falha no servidor</response>

        [ProducesResponseType(typeof(ResultViewModel<IEnumerable<Post>>), StatusCodes.Status200OK)]
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

        /// <summary>
        /// Obter detalhes de uma postagem
        /// </summary>
        /// <param name="id">Identificador da postagem</param>
        /// <returns>Dados da postagem</returns>
        /// <response code="200">Sucesso</response>
        /// <response code="400">Identificador inválido</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="404">Postagem não encontrada</response>
        /// <response code="500">Falha no servidor</response>

        [ProducesResponseType(typeof(ResultViewModel<Post>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Obter postagens de uma categoria
        /// </summary>
        /// <param name="categoryName">Nome da categoria</param>
        /// <param name="page">Página de busca</param>
        /// <param name="pageSize">Quantidade de registros por página</param>
        /// <returns>Coleção de postagens</returns>
        /// <response code="200">Sucesso</response>
        /// <response code="400">Categoria inválida</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="404">Nenhuma postagem encontrada</response>
        /// <response code="500">Falha no servidor</response>

        [ProducesResponseType(typeof(ResultViewModel<Post>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Cadastrar uma postagem
        /// </summary>
        /// <param name="model">Dados da postagem</param>
        /// <returns>Postagem recém criada</returns>
        /// <response code="201">Sucesso</response>
        /// <response code="400">Postagem inválida</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="403">Sem permissão</response>
        /// <response code="500">Falha no servidor</response>

        [ProducesResponseType(typeof(ResultViewModel<Post>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
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
                return StatusCode(500, new ResultViewModel<string>("Erro ao salvar a postagem"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>(new List<string> { "Erro interno no servidor", ex.Message }));
            }
        }

        /// <summary>
        /// Atualiza uma postagem
        /// </summary>
        /// <param name="id">Identificador da postagem</param>
        /// <param name="model">Dados da postagem</param>
        /// <returns>Postagem atualizada</returns>
        /// <response code="204">Sucesso</response>
        /// <response code="400">Postagem inválida</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="403">Sem permissão</response>
        /// <response code="404">Postagem não encontrada</response>
        /// <response code="500">Falha no servidor</response>

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Remove uma postagem
        /// </summary>
        /// <param name="id">Identificador da postagem</param>
        /// <returns>Postagem exclúida</returns>
        /// <response code="204">Sucesso</response>
        /// <response code="400">Identificador inválido</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="403">Sem permissão</response>
        /// <response code="404">Postagem não encontrada</response>

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultViewModel<string>), StatusCodes.Status500InternalServerError)]
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
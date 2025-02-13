using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Posts
{
    public class CreatePostViewModel
    {
        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(180, MinimumLength = 3, ErrorMessage = "O título deve conter entre 3 a 180 caracteres.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "O resumo é obrigatório")]
        [MaxLength(ErrorMessage = "O resumo deve conter até 400 caracteres.")]
        public string Summary { get; set; }
        [Required(ErrorMessage = "O texto da postagem é obrigatória")]
        public string Body { get; set; }
        [Required(ErrorMessage = "O slug é obrigatório")]
        public string Slug { get; set; }
        [Required(ErrorMessage = "O identificador da categoria é obrigatório")]
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "O identificador do autor da postagem é obrigatório")]
        public int AuthorId { get; set; }
    }
}

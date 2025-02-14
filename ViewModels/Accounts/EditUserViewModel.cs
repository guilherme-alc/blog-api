using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Accounts
{
    public class EditUserViewModel
    {
        [MaxLength(80)]
        public string Name { get; set; }
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; }
        public string Bio { get; set; }
        [MaxLength(255)]
        public string Password { get; set; }
    }
}

using Blog.Models;

namespace Blog.ViewModels.Users
{
    public class ListUserViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Slug { get; set; }
        public DateTime CreateDate { get; set; }
        public IList<Role> Roles { get; set; }
    }
}

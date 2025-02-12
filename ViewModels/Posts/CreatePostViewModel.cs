namespace Blog.ViewModels.Posts
{
    public class CreatePostViewModel
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Body { get; set; }
        public string Slug { get; set; }
        public int CategoryId { get; set; }
        public int AuthorId { get; set; }
    }
}

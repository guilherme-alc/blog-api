using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Blog.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Body { get; set; }
        public string Slug { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        [JsonIgnore]
        public int AuthorId { get; set; }
        public User Author { get; set; }
        [JsonIgnore]
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public IList<Tag> Tags { get; set; }
    }
}

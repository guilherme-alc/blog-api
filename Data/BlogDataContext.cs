using Blog.Data.Mappings;
using Blog.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data
{
    public class BlogDataContext : DbContext
    {
        public BlogDataContext(DbContextOptions<BlogDataContext> options)
            : base(options)
        {
            
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CategoryMap());
            modelBuilder.ApplyConfiguration(new UserMap());
            modelBuilder.ApplyConfiguration(new PostMap());
            modelBuilder.ApplyConfiguration(new RoleMap());
            modelBuilder.ApplyConfiguration(new TagMap());

            /*modelBuilder.Entity<Post>()
                        .HasMany(p => p.Tags)
                        .WithMany(t => t.Posts)
                        .UsingEntity(
                            "PostTag",

                            post => post.HasOne(typeof(Tag))
                                    .WithMany()
                                    .HasForeignKey("PostId")
                                    .HasConstraintName("FK_PostTag_PostId"),

                            tag => tag.HasOne(typeof(Post))
                                    .WithMany()
                                    .HasForeignKey("TagId")
                                    .HasConstraintName("FK_PostTag_TagId"),

                            keys => keys.HasKey("PostId", "TagId")
                        );*/
        }
    }
}

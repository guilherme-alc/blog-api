using Blog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Data.Mappings
{
    public class PostMap : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable("Post");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();

            builder.Property(p => p.Title)
                .IsRequired()
                .HasColumnType("VARCHAR")
                .HasMaxLength(160);

            builder.Property(p => p.Summary)
                .IsRequired()
                .HasColumnType("VARCHAR")
                .HasMaxLength(255);

            builder.Property(p => p.Body)
                .IsRequired()
                .HasColumnType("TEXT");

            builder.Property(p => p.CreateDate)
                .IsRequired()
                .HasColumnType("SMALLDATETIME")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(p => p.LastUpdateDate)
                .IsRequired()
                .HasColumnType("SMALLDATETIME")
                .HasDefaultValueSql("GETDATE()");
                //HasDefaultValue(DateTime.Now.ToUniversalTime());


            builder.HasIndex(u => u.Slug, "IX_Post_Slug")
                .IsUnique();

            // Relacionamento 1:n
            builder.HasOne(p => p.Author)
                .WithMany(a => a.Posts)
                .HasForeignKey(p => p.AuthorId)  // Especificando a chave estrangeira explicitamente
                .HasConstraintName("FK_Post_Author") // Gera a Constraint com o nome informado
                .OnDelete(DeleteBehavior.Cascade); // Caso um autor seja excluidos, todos os posts relacionados a ele também serão

            builder.HasOne(p => p.Category)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CategoryId)
                .HasConstraintName("FK_Category_Category")
                .OnDelete(DeleteBehavior.Cascade);

            // Relacionamento n:n
            builder.HasMany(t => t.Tags)
                .WithMany(p => p.Posts)
                .UsingEntity<Dictionary<string, object>>( // Entidade virtual
                    // Nome da tabela
                    "PostTag",

                    tag => tag.HasOne<Tag>()
                        .WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("FK_PostTag_TagId")
                        .OnDelete(DeleteBehavior.Cascade),

                    post => post.HasOne<Post>()
                        .WithMany()
                        .HasForeignKey("PostId")
                        .HasConstraintName("FK_PostTag_PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                );

                                                     
        }
    }
}

using Blog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Data.Mappings
{
    public class CategoryMap : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            // Específica a tabela, pois o Entity Framework vai tentar pluralizar. Equivalente ao [Table("Category"]
            builder.ToTable("Category");
            // Informa a chave primária
            builder.HasKey(c => c.Id);
            // Informa a propriedade
            builder.Property(c => c.Id)
                .ValueGeneratedOnAdd() // Indica que o valor da propriedade será gerado automaticamente pelo banco (funcionaria sem)
                .UseIdentityColumn(); // Equivalente ao IDENTITY (1,1) no SQL

            // Demais propriedades
            builder.Property(c => c.Name)
                .IsRequired() // NOT NULL
                .HasColumnName("Name") // Como no banco a coluna tem o mesmo nome que a propriedade, não seria necessário
                .HasColumnType("NVARCHAR")
                .HasMaxLength(80);

            builder.Property(c => c.Slug)
                .IsRequired()
                .HasColumnName("Slug")
                .HasColumnType("VARCHAR")
                .HasMaxLength(80);

            // Índices - Faz o mapeamento de um índice existente ou cria um novo caso não exista a partir das migrations
            builder.HasIndex(c => c.Slug, "IX_Category_Slug")
                .IsUnique();
        }
    }
}

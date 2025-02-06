using Blog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Data.Mappings
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Table
            builder.ToTable("User");
            // Primary Key
            builder.HasKey(u => u.Id);
            // Identity
            builder.Property(u => u.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();

            builder.Property(u => u.Name)
                .IsRequired()
                .HasColumnType("NVARCHAR")
                .HasMaxLength(80);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasColumnType("VARCHAR")
                .HasMaxLength(200);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasColumnType("VARCHAR")
                .HasMaxLength(255);

            builder.Property(u => u.Bio)
                .IsRequired(false)
                .HasColumnType("TEXT");

            builder.Property(u => u.Image)
              .IsRequired(false)
              .HasColumnType("VARCHAR")
              .HasMaxLength(2000);

            builder.Property(u => u.Slug)
                .IsRequired()
                .HasColumnName("Slug")
                .HasColumnType("VARCHAR")
                .HasMaxLength(80);

            builder.HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    role => role.HasOne<Role>()
                                .WithMany()
                                .HasForeignKey("RoleId")
                                .HasConstraintName("FK_UserRole_RoleId")
                                .OnDelete(DeleteBehavior.Cascade),

                    user => user.HasOne<User>()
                                .WithMany()
                                .HasForeignKey("UserId")
                                .HasConstraintName("FK_UserRole_UserId")
                                .OnDelete(DeleteBehavior.Cascade)
                );

            // Índice
            builder.HasIndex(u => u.Slug, "IX_User_Slug")
                .IsUnique();
            builder.HasIndex(u => u.Email, "IX_User_Email")
                .IsUnique();
        }
    }
}

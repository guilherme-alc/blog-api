using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGithubFromUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Github",
                table: "User");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Github",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trivia.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueQuestionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Questions_Question",
                table: "Questions",
                column: "Question",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_Question",
                table: "Questions");
        }
    }
}

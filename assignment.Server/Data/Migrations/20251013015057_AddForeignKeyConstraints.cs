using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObituaryApplication.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeyConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add foreign key constraint for Obituaries.CreatorId -> AspNetUsers.Id
            migrationBuilder.AddForeignKey(
                name: "FK_Obituaries_AspNetUsers_CreatorId",
                table: "Obituaries",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_Obituaries_AspNetUsers_CreatorId",
                table: "Obituaries");
        }
    }
}

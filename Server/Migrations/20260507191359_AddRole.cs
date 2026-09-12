using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class AddRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_Users_UserId1",
                table: "GroupMembers");

            migrationBuilder.DropIndex(
                name: "IX_GroupMembers_UserId1",
                table: "GroupMembers");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "GroupMembers");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "GroupMembers",
                type: "int",
                nullable: false,
                defaultValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "GroupMembers");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "GroupMembers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UserId1",
                table: "GroupMembers",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_Users_UserId1",
                table: "GroupMembers",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

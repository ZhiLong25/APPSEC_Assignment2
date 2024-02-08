using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APPSEC_Assignment2.Migrations
{
    public partial class PasswordHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PasswordHistories_AspNetUsers_RegisterId",
                table: "PasswordHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PasswordHistories",
                table: "PasswordHistories");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "PasswordHistories",
                newName: "PasswordHistory");

            migrationBuilder.RenameIndex(
                name: "IX_PasswordHistories_RegisterId",
                table: "PasswordHistory",
                newName: "IX_PasswordHistory_RegisterId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PasswordHistory",
                table: "PasswordHistory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordHistory_AspNetUsers_RegisterId",
                table: "PasswordHistory",
                column: "RegisterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PasswordHistory_AspNetUsers_RegisterId",
                table: "PasswordHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PasswordHistory",
                table: "PasswordHistory");

            migrationBuilder.RenameTable(
                name: "PasswordHistory",
                newName: "PasswordHistories");

            migrationBuilder.RenameIndex(
                name: "IX_PasswordHistory_RegisterId",
                table: "PasswordHistories",
                newName: "IX_PasswordHistories_RegisterId");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PasswordHistories",
                table: "PasswordHistories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordHistories_AspNetUsers_RegisterId",
                table: "PasswordHistories",
                column: "RegisterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}

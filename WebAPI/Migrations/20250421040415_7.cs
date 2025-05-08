using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class _7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentExams_AdviceRules_AdviceRulesId",
                table: "StudentExams");

            migrationBuilder.DropIndex(
                name: "IX_StudentExams_AdviceRulesId",
                table: "StudentExams");

            migrationBuilder.DropColumn(
                name: "AdviceRulesId",
                table: "StudentExams");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AdviceRules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "AdviceRules");

            migrationBuilder.AddColumn<Guid>(
                name: "AdviceRulesId",
                table: "StudentExams",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_AdviceRulesId",
                table: "StudentExams",
                column: "AdviceRulesId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentExams_AdviceRules_AdviceRulesId",
                table: "StudentExams",
                column: "AdviceRulesId",
                principalTable: "AdviceRules",
                principalColumn: "Id");
        }
    }
}

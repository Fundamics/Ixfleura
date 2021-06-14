using Microsoft.EntityFrameworkCore.Migrations;

namespace Ixfleura.Data.Migrations
{
    public partial class AddSuggestions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "channel_id",
                table: "suggestions",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "channel_id",
                table: "suggestions");
        }
    }
}

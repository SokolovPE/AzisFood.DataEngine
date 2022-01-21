using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzisFood.DataEngine.ManualTest.Migrations.CatalogMigrations;

public partial class InitializeCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "Categories",
            table => new
            {
                Id = table.Column<Guid>("uuid", nullable: false),
                Title = table.Column<string>("text", nullable: false),
                Order = table.Column<int>("integer", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Categories", x => x.Id); });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "Categories");
    }
}
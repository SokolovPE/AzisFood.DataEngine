using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzisFood.DataEngine.ManualTest.Migrations.PostgresMigrations;

public partial class UpdateOrderPostgres : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "Orders",
            table => new
            {
                Id = table.Column<Guid>("uuid", nullable: false),
                OrderDate = table.Column<DateTimeOffset>("timestamp with time zone", nullable: false),
                Price = table.Column<decimal>("numeric", nullable: false),
                Qty = table.Column<int>("integer", nullable: false),
                Description = table.Column<string>("text", nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_Orders", x => x.Id); });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "Orders");
    }
}
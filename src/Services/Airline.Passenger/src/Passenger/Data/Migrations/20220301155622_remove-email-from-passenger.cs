using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passenger.Data.Migrations
{
    public partial class removeemailfrompassenger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                schema: "dbo",
                table: "Passenger");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "dbo",
                table: "Passenger",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

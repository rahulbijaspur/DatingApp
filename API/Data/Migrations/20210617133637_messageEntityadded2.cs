using Microsoft.EntityFrameworkCore.Migrations;

namespace API.Migrations
{
    public partial class messageEntityadded2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SenderUesrname",
                table: "Messages",
                newName: "SenderUsername");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SenderUsername",
                table: "Messages",
                newName: "SenderUesrname");
        }
    }
}

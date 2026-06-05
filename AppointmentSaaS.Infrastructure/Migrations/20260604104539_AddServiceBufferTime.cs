using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentSaaS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceBufferTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BufferTimeMinutes",
                table: "Services",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BufferTimeMinutes",
                table: "Services");
        }
    }
}

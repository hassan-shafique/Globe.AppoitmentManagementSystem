using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentSaaS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffRoleAndSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Staff",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "Staff",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "Staff");
        }
    }
}

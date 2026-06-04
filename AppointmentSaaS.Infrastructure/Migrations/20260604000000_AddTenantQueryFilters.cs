using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentSaaS.Infrastructure.Migrations
{
    /// <summary>
    /// Checkpoint migration for tenant query filter registration.
    /// No schema changes — global query filters are applied in OnModelCreating and
    /// are invisible to the EF migration system.
    /// </summary>
    public partial class AddTenantQueryFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) { }

        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}

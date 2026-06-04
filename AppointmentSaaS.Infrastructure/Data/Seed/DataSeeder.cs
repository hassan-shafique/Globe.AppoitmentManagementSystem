using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Enums;
using AppointmentSaaS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppointmentSaaS.Infrastructure.Data.Seed;

public class DataSeeder(
    AppDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ILogger<DataSeeder> logger)
{
    public async Task SeedAsync()
    {
        try
        {
            await context.Database.MigrateAsync();
            await SeedRolesAsync();
            await SeedDefaultTenantAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database seeding");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in Enum.GetNames<UserRole>())
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
                logger.LogInformation("Created role: {Role}", roleName);
            }
        }
    }

    private async Task SeedDefaultTenantAsync()
    {
        if (await context.Tenants.AnyAsync()) return;

        var tenant = Tenant.Create("Globe Appointments", "globe", "admin@globe.com");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var adminUser = new ApplicationUser
        {
            UserName = "admin@globe.com",
            Email = "admin@globe.com",
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "User"
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, UserRole.TenantAdmin.ToString());
            var appUser = AppUser.Create(adminUser.Id, tenant.Id, "Admin", "User", "admin@globe.com", UserRole.TenantAdmin);
            context.AppUsers.Add(appUser);

            var staffRecord = Staff.Create(tenant.Id, adminUser.Id, "Admin", "Staff", "admin@globe.com");
            context.Set<Staff>().Add(staffRecord);

            await context.SaveChangesAsync();
            logger.LogInformation("Seeded default tenant, admin user, and staff record (Staff.Id={StaffId})", staffRecord.Id);
        }
    }
}

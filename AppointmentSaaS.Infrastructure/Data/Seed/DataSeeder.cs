using AppointmentSaaS.Domain.Entities;
using AppointmentSaaS.Domain.Enums;
using AppointmentSaaS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppointmentSaaS.Infrastructure.Data.Seed;

public class DataSeeder(
    AppDbContext context,
    UserManager<ApplicationIdentityUser> userManager,
    RoleManager<IdentityRole> roleManager,
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
        foreach (var role in Enum.GetNames<UserRole>())
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role: {Role}", role);
            }
        }
    }

    private async Task SeedDefaultTenantAsync()
    {
        if (await context.Tenants.AnyAsync()) return;

        var tenant = Tenant.Create("Globe Appointments", "globe", "admin@globe.com");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var adminUser = new ApplicationIdentityUser
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
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded default tenant and admin user");
        }
    }
}

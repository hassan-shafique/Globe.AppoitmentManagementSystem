using Microsoft.AspNetCore.Identity;

namespace AppointmentSaaS.Infrastructure.Identity;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }

    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }
    public ApplicationRole(string roleName, string description) : base(roleName)
        => Description = description;
}

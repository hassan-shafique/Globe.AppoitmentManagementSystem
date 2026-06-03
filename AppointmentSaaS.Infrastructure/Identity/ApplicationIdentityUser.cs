using Microsoft.AspNetCore.Identity;

namespace AppointmentSaaS.Infrastructure.Identity;

public class ApplicationIdentityUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

using AppointmentSaaS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AppointmentSaaS.Infrastructure.Services;

/// <summary>
/// Resolves the current tenant from the HTTP request context.
/// Resolution order:
///   1. JWT claim "tenantId" (authenticated requests)
///   2. "X-Tenant-Id" request header (machine-to-machine or public API)
///   3. null — no tenant context (super-admin, background services)
/// </summary>
public class TenantResolver(IHttpContextAccessor httpContextAccessor) : ITenantProvider
{
    public Guid? TenantId
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            if (context is null) return null;

            // 1. JWT claim set at login time — most reliable source
            var claim = context.User.FindFirst("tenantId")?.Value
                     ?? context.User.FindFirst(ClaimTypes.GroupSid)?.Value;
            if (claim is not null && Guid.TryParse(claim, out var fromClaim))
                return fromClaim;

            // 2. Explicit header for service-to-service or unauthenticated public routes
            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var header)
                && Guid.TryParse(header.ToString(), out var fromHeader))
                return fromHeader;

            return null;
        }
    }
}

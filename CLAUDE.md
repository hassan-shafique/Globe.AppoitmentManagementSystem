# Globe Appointment Management System

Multi-tenant appointment SaaS built with Clean Architecture (.NET 9, EF Core, SQLite, ASP.NET Identity, JWT).

## Solution Structure

| Project | Purpose |
|---|---|
| `AppointmentSaaS.Domain` | Entities, domain events, enums, exceptions — zero external dependencies |
| `AppointmentSaaS.Application` | CQRS (MediatR), DTOs, FluentValidation, service interfaces |
| `AppointmentSaaS.Infrastructure` | EF Core, Identity, JWT, repositories, email, seeding |
| `AppointmentSaaS.Web` | Razor Pages, API controllers, middleware, DI composition root |
| `AppointmentSaaS.Tests` | xUnit unit tests (Moq, FluentAssertions) |

## Auth & Identity

### Identity Types
- `ApplicationUser : IdentityUser` — extends with `FirstName`, `LastName`, `FullName`
- `ApplicationRole : IdentityRole` — extends with `Description`
- `AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>`

### Auth Flow
1. **Register** → `POST /api/auth/register` — creates Identity user (`EmailConfirmed=false`), sends verification email, returns `RegisterResponse`
2. **Verify Email** → `POST /api/auth/verify-email` — confirms email token, enables login
3. **Login** → `POST /api/auth/login` — requires `EmailConfirmed=true`, returns JWT + refresh token in HttpOnly cookie
4. **Forgot Password** → `POST /api/auth/forgot-password` — sends reset token email (avoids user enumeration)
5. **Reset Password** → `POST /api/auth/reset-password` — validates token, resets password
6. **Refresh Token** → `POST /api/auth/refresh`
7. **Revoke / Logout** → `POST /api/auth/revoke`, `POST /api/auth/logout`

### Identity Configuration (`DependencyInjection.cs`)
```
RequireDigit=true, RequireLowercase=true, RequireUppercase=true,
RequireNonAlphanumeric=true, RequiredLength=8,
RequireUniqueEmail=true, RequireConfirmedEmail=true
```

## Multi-Tenancy

Shared SQLite database with `TenantId` on all business data and global EF Core query filters enforcing row-level isolation per request.

### Tenant-scoped entities
`AppUser`, `Business`, `Staff`, `Service`, `Customer`, `Appointment` — all carry a non-nullable `TenantId` FK to the `Tenants` table.

### Tenant resolution
`TenantResolver` (implements `ITenantProvider`) resolves the current tenant per request:
1. JWT claim `tenantId` — set at login, most requests
2. `X-Tenant-Id` header — machine-to-machine / unauthenticated public routes
3. `null` — super-admin or background service (query filter bypassed)

### Global query filters (`AppDbContext.OnModelCreating`)
Entities with soft-delete + tenant isolation:
```csharp
HasQueryFilter(e => !e.IsDeleted && (tenantId == null || e.TenantId == tenantId))
```
`Appointment` (no soft-delete):
```csharp
HasQueryFilter(e => tenantId == null || e.TenantId == tenantId)
```
`Tenant` table: soft-delete only (it IS the root tenant record).

### Bypassing filters
Use `dbContext.Set<T>().IgnoreQueryFilters()` for cross-tenant admin queries.

### Migrations
- `AddTenantQueryFilters` (2026-06-04) — checkpoint; no schema change (filters are code-only).

## Key Interfaces

| Interface | Impl | Notes |
|---|---|---|
| `IIdentityService` | `IdentityService` | Create user, validate, email confirm, password reset |
| `IEmailService` | `EmailService` | Log-only stub — replace with real provider |
| `ITokenService` | `TokenService` | JWT + refresh token generation |
| `ICurrentUserService` | `CurrentUserService` | Extracts user from `HttpContext` |
| `ITenantProvider` | `TenantResolver` | Current tenant ID for query filtering |

## Database

- **Provider:** SQLite (`Data Source=AppointmentSaaS.db`)
- **Migrations:** `AppointmentSaaS.Infrastructure/Migrations/`
  - `InitialCreate` — full schema including Identity tables
  - `AddApplicationRoleAndIdentityV2` — adds `Description` column to `AspNetRoles`
  - `AddTenantQueryFilters` — checkpoint migration (no schema change)
- **Seeding:** `DataSeeder` seeds roles + default admin (`admin@globe.com` / `Admin@123!`)

### Adding a migration
```bash
dotnet ef migrations add <MigrationName> \
  --project AppointmentSaaS.Infrastructure \
  --startup-project AppointmentSaaS.Web
```

## Running Tests
```bash
dotnet test AppointmentSaaS.Tests/
```
40 tests — Domain, Application (Auth, Appointments), Infrastructure (Repositories).

## Architecture Notes

- **Soft delete + tenant isolation** — combined EF global query filters; soft-delete on 5 entities, tenant filter on 6 (including Appointment)
- **Multi-tenancy** — `TenantId` FK + JWT claims; login scoped to `TenantSlug`
- **CQRS** — MediatR commands/queries with `ValidationBehaviour` + `LoggingBehaviour`
- **Domain events** — `AppointmentCreatedEvent`, `AppointmentCancelledEvent` on aggregate root
- **JWT** — Bearer + HttpOnly cookie (`access_token`); refresh token rotation with revocation
- **Password reset / email verification** — anti-enumeration: always return 200 regardless of whether user exists

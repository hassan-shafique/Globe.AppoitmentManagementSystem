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

## Business Management Module

### Business Types (`AppointmentSaaS.Domain/Enums/BusinessType.cs`)
`Generic`, `Doctor`, `Dentist`, `Teacher`, `Tutor`, `Salon`, `Consultant`

### Business entity (`AppointmentSaaS.Domain/Entities/Business.cs`)
- Inherits `SoftDeleteEntity`; scoped to a `Tenant` via `TenantId`
- Factory: `Business.Create(tenantId, name, type, address?, city?, phone?, email?)`
- Methods: `Update(...)`, `Activate()`, `Deactivate()`, `SoftDelete()`, `Restore()`

### CQRS (`AppointmentSaaS.Application/Features/Businesses/`)
| Command/Query | Returns |
|---|---|
| `CreateBusinessCommand` | `BusinessDto` |
| `UpdateBusinessCommand` | `BusinessDto` |
| `DeleteBusinessCommand` | void (soft-delete) |
| `ActivateBusinessCommand` | void |
| `DeactivateBusinessCommand` | void |
| `GetBusinessByIdQuery` | `BusinessDto` |
| `GetBusinessesByTenantQuery` | `IReadOnlyList<BusinessDto>` |
| `GetActiveBusinessesByTenantQuery` | `IReadOnlyList<BusinessDto>` |

### API endpoints (`/api/businesses`)
| Method | Route | Roles |
|---|---|---|
| GET | `/api/businesses` | Authenticated |
| GET | `/api/businesses/active` | Authenticated |
| GET | `/api/businesses/{id}` | Authenticated |
| POST | `/api/businesses` | TenantAdmin, SuperAdmin |
| PUT | `/api/businesses/{id}` | TenantAdmin, SuperAdmin |
| DELETE | `/api/businesses/{id}` | TenantAdmin, SuperAdmin |
| PATCH | `/api/businesses/{id}/activate` | TenantAdmin, SuperAdmin |
| PATCH | `/api/businesses/{id}/deactivate` | TenantAdmin, SuperAdmin |

### Razor Pages (`/Businesses/`)
`Index`, `Create`, `Edit`, `Details`

### Migration needed
`BusinessType` column was added to `Businesses` table. Run:
```bash
dotnet ef migrations add AddBusinessType \
  --project AppointmentSaaS.Infrastructure \
  --startup-project AppointmentSaaS.Web
```

## Service Management Module

### Service entity (`AppointmentSaaS.Domain/Entities/Service.cs`)
- Inherits `SoftDeleteEntity`; scoped to a `Tenant` via `TenantId`
- Optionally scoped to a `Business` via `BusinessId` (null = all locations)
- Factory: `Service.Create(tenantId, name, description, durationMinutes, price, bufferTimeMinutes?, businessId?)`
- Methods: `Update(...)`, `Activate()`, `Deactivate()`, `SoftDelete()`, `Restore()`

### Fields
| Field | Type | Notes |
|---|---|---|
| `Name` | string | Required, max 200 chars |
| `Description` | string? | Optional, max 1000 chars |
| `DurationMinutes` | int | Must be > 0 |
| `Price` | decimal | Must be ≥ 0 |
| `BufferTimeMinutes` | int | Minutes blocked after appointment ends, ≥ 0 |
| `IsActive` | bool | Controls booking availability |

### CQRS (`AppointmentSaaS.Application/Features/Services/`)
| Command/Query | Returns |
|---|---|
| `CreateServiceCommand` | `ServiceDto` |
| `UpdateServiceCommand` | `ServiceDto` |
| `DeleteServiceCommand` | void (soft-delete) |
| `ActivateServiceCommand` | void |
| `DeactivateServiceCommand` | void |
| `GetServiceByIdQuery` | `ServiceDto` |
| `GetServicesByTenantQuery` | `IReadOnlyList<ServiceDto>` |
| `GetActiveServicesByTenantQuery` | `IReadOnlyList<ServiceDto>` |
| `GetServicesByBusinessQuery` | `IReadOnlyList<ServiceDto>` |

### API endpoints (`/api/services`)
| Method | Route | Roles |
|---|---|---|
| GET | `/api/services` | Authenticated |
| GET | `/api/services/active` | Authenticated |
| GET | `/api/services/{id}` | Authenticated |
| GET | `/api/services/business/{businessId}` | Authenticated |
| POST | `/api/services` | TenantAdmin, SuperAdmin |
| PUT | `/api/services/{id}` | TenantAdmin, SuperAdmin |
| DELETE | `/api/services/{id}` | TenantAdmin, SuperAdmin |
| PATCH | `/api/services/{id}/activate` | TenantAdmin, SuperAdmin |
| PATCH | `/api/services/{id}/deactivate` | TenantAdmin, SuperAdmin |

### Razor Pages (`/Services/`)
`Index`, `Create`, `Edit`, `Details`

### Migration
`AddServiceBufferTime` — adds `BufferTimeMinutes` column to `Services` table.

## Staff Management Module

### Staff entity (`AppointmentSaaS.Domain/Entities/Staff.cs`)
- Inherits `SoftDeleteEntity`; scoped to a `Tenant` via `TenantId`
- Optionally scoped to a `Business` via `BusinessId` (null = tenant-wide)
- Factory: `Staff.Create(tenantId, identityUserId, firstName, lastName, email, phone?, role?, skills?, businessId?)`
- Methods: `Update(...)`, `Activate()`, `Deactivate()`, `AssignToBusiness(...)`, `SoftDelete()`, `Restore()`

### Fields
| Field | Type | Notes |
|---|---|---|
| `FirstName` | string | Required, max 100 chars |
| `LastName` | string | Required, max 100 chars |
| `Email` | string | Required, max 256 chars |
| `Phone` | string? | Optional, max 30 chars |
| `Bio` | string? | Optional description |
| `Role` | string? | Job role/title, max 100 chars |
| `Skills` | string? | Comma-separated specialisations, max 500 chars |
| `IsActive` | bool | Controls booking availability |

### CQRS (`AppointmentSaaS.Application/Features/Staff/`)
| Command/Query | Returns |
|---|---|
| `CreateStaffCommand` | `StaffDto` |
| `UpdateStaffCommand` | `StaffDto` |
| `DeleteStaffCommand` | void (soft-delete) |
| `ActivateStaffCommand` | void |
| `DeactivateStaffCommand` | void |
| `GetStaffByIdQuery` | `StaffDto` |
| `GetStaffByTenantQuery` | `IReadOnlyList<StaffDto>` |
| `GetActiveStaffByTenantQuery` | `IReadOnlyList<StaffDto>` |
| `GetStaffByBusinessQuery` | `IReadOnlyList<StaffDto>` |

### API endpoints (`/api/staff`)
| Method | Route | Roles |
|---|---|---|
| GET | `/api/staff` | Authenticated |
| GET | `/api/staff/active` | Authenticated |
| GET | `/api/staff/{id}` | Authenticated |
| GET | `/api/staff/business/{businessId}` | Authenticated |
| POST | `/api/staff` | TenantAdmin, SuperAdmin |
| PUT | `/api/staff/{id}` | TenantAdmin, SuperAdmin |
| DELETE | `/api/staff/{id}` | TenantAdmin, SuperAdmin |
| PATCH | `/api/staff/{id}/activate` | TenantAdmin, SuperAdmin |
| PATCH | `/api/staff/{id}/deactivate` | TenantAdmin, SuperAdmin |

### Razor Pages (`/Staff/`)
`Index`, `Create`, `Edit`, `Details`

### Migration
`AddStaffRoleAndSkills` — adds `Role` and `Skills` columns to `Staff` table.

## Running Tests
```bash
dotnet test AppointmentSaaS.Tests/
```
~105 tests — Domain (Appointment, Business, Service, Staff), Application (Auth, Appointments, Businesses, Services, Staff), Infrastructure (Repositories).

## Architecture Notes

- **Soft delete + tenant isolation** — combined EF global query filters; soft-delete on 5 entities, tenant filter on 6 (including Appointment)
- **Multi-tenancy** — `TenantId` FK + JWT claims; login scoped to `TenantSlug`
- **CQRS** — MediatR commands/queries with `ValidationBehaviour` + `LoggingBehaviour`
- **Domain events** — `AppointmentCreatedEvent`, `AppointmentCancelledEvent` on aggregate root
- **JWT** — Bearer + HttpOnly cookie (`access_token`); refresh token rotation with revocation
- **Password reset / email verification** — anti-enumeration: always return 200 regardless of whether user exists

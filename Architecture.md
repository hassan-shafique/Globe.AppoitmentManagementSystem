# Architecture — Globe Appointment Management System

## Overview

Globe Appointment Management System is built following **Clean Architecture** (also known as Onion Architecture), ensuring strict separation of concerns and testability.

```
┌───────────────────────────────────────────────────────┐
│                    Presentation                        │
│         AppointmentSaaS.Web                           │
│   (Razor Pages + REST API + Middleware)               │
└──────────────────────┬────────────────────────────────┘
                       │ depends on
┌──────────────────────▼────────────────────────────────┐
│                   Application                          │
│         AppointmentSaaS.Application                   │
│   (CQRS Handlers, DTOs, Validators, Interfaces)       │
└──────────────────────┬────────────────────────────────┘
                       │ depends on
┌──────────────────────▼────────────────────────────────┐
│                     Domain                             │
│           AppointmentSaaS.Domain                      │
│   (Entities, Domain Events, Exceptions, Interfaces)   │
└───────────────────────────────────────────────────────┘
                       ▲ implemented by
┌──────────────────────┴────────────────────────────────┐
│                  Infrastructure                        │
│         AppointmentSaaS.Infrastructure                │
│   (EF Core, Identity, JWT, Repositories, Services)    │
└───────────────────────────────────────────────────────┘
```

## Layer Responsibilities

### Domain (AppointmentSaaS.Domain)
The innermost layer. Zero external dependencies.

- **Base Classes** — `BaseEntity`, `AuditableEntity`, `SoftDeleteEntity` (Common/)
- **Entities** — Business objects with encapsulated behavior and invariants
- **Value Objects** — Immutable objects identified by value, not ID
- **Domain Events** — Signals that something significant happened (decoupled side effects)
- **Enumerations** — Business-meaningful enums (`AppointmentStatus`, `UserRole`)
- **Exceptions** — Domain rule violations (`DomainException`, `AppointmentConflictException`)
- **Interfaces** — Contracts (`IRepository<T>`, `IUnitOfWork`, `IAppointmentRepository`)

### Application (AppointmentSaaS.Application)
Orchestration layer. Depends only on Domain.

- **CQRS Handlers** — Commands (mutations) and Queries (reads) via MediatR
- **Pipeline Behaviours** — Cross-cutting concerns: logging, validation
- **DTOs** — Data transfer objects for layer boundaries
- **Validators** — FluentValidation validators for commands/queries
- **Service Interfaces** — `IJwtService`, `ICurrentUserService`, `IEmailService`
- **AutoMapper Profiles** — Entity → DTO projections

### Infrastructure (AppointmentSaaS.Infrastructure)
Implements Application and Domain interfaces. Adapts external systems.

- **AppDbContext** — EF Core context inheriting from `IdentityDbContext`
- **Configurations** — `IEntityTypeConfiguration<T>` for all entities
- **Repository Implementations** — `Repository<T>`, `AppointmentRepository`, `TenantRepository`
- **UnitOfWork** — Transaction management over EF Core
- **Identity** — `ApplicationIdentityUser` extending `IdentityUser`
- **TokenService / JwtService** — JWT generation and refresh token creation
- **CurrentUserService** — Extracts user context from `HttpContext`
- **EmailService** — Email notification stub (ready for SMTP/SendGrid integration)
- **DataSeeder** — Initial role and tenant seed data

### Web (AppointmentSaaS.Web)
Entry point. Depends on Application + Infrastructure (for DI wiring only).

- **Razor Pages** — Server-side rendered UI (Login, Register, Dashboard, Appointments)
- **API Controllers** — RESTful endpoints for headless consumers
- **Middleware** — `ExceptionHandlingMiddleware` for consistent JSON error responses
- **Program.cs** — DI container wiring, pipeline configuration

## Domain Model Base Class Hierarchy

```
BaseEntity
  ├── AuditableEntity  (+ CreatedAt/By, UpdatedAt/By)
  │     ├── SoftDeleteEntity  (+ IsDeleted, DeletedAt, DeletedBy)
  │     │     ├── Tenant
  │     │     ├── Business
  │     │     ├── AppUser
  │     │     ├── Staff
  │     │     ├── Customer
  │     │     └── Service
  │     ├── Appointment
  │     └── SubscriptionPlan
  └── RefreshToken
```

## Domain Model Relationships

```
SubscriptionPlan (1) ──< Tenant (M)

Tenant (1) ──< Business (M)
Tenant (1) ──< AppUser (M)
Tenant (1) ──< Staff (M)
Tenant (1) ──< Service (M)
Tenant (1) ──< Customer (M)
Tenant (1) ──< Appointment (M)

Business (1) ──< Staff (M)        [optional — staff may be tenant-wide]
Business (1) ──< Service (M)      [optional — service may be tenant-wide]
Business (1) ──< Appointment (M)  [optional]

Staff (1) ──< Appointment (M)
Service (1) ──< Appointment (M)

[Appointment client — exactly one of:]
  Customer (1) ──< Appointment (M)   walk-in / Customer-entity path
  AppUser  (1) ──< Appointment (M)   registered-user / portal path

AppUser (1) ──< RefreshToken (M)
Customer (M) >──  AppUser (1)        optional link when customer self-registers
```

## Soft Delete Strategy

Entities extending `SoftDeleteEntity` are **never physically removed** from the database:

1. Call `entity.SoftDelete(deletedBy)` in the Application or Domain layer.
2. EF Core saves `IsDeleted = true`, `DeletedAt`, and `DeletedBy`.
3. A **global query filter** (`e => !e.IsDeleted`) in `AppDbContext.OnModelCreating` ensures deleted rows are excluded from all normal queries.
4. To query deleted records explicitly use `.IgnoreQueryFilters()`.
5. `Restore()` reverses the soft delete if the business requires it.

## Multi-Tenancy Design

Tenant isolation is enforced at the data level:

```
Request → JWT Token → Claims (tenantId) → ICurrentUserService → Application Handlers → Filtered Queries
```

- Each entity holding tenant-scoped data carries a `TenantId` foreign key
- `CurrentUserService` reads `tenantId` from JWT claims
- Application handlers extract `TenantId` from `ICurrentUserService` and scope all queries/mutations

## JWT Authentication Design

### Service: `JwtService` (`IJwtService`)

| Method | Description |
|--------|-------------|
| `GenerateAccessToken(user, email, roles)` | Creates a signed JWT (HS256) with `sub`, `email`, `jti`, `tenantId`, `fullName`, and role claims. Expiry from `JwtSettings:ExpiryMinutes` (default 60 min). |
| `GenerateRefreshToken(appUserId)` | Creates an opaque 64-byte Base64 token via `RandomNumberGenerator`. Returns a `RefreshToken` domain object (7-day expiry). **Not yet persisted — caller must save via repository.** |
| `GetUserIdFromExpiredToken(token)` | Validates JWT signature only (ignores lifetime). Used by `RefreshTokenCommand` to identify the user from an expired access token. |

### Entity: `RefreshToken` (BaseEntity)

```
Token          — opaque secret string (unique index)
AppUserId      — FK to AppUser (cascade delete)
ExpiresAt      — UTC expiry
IsRevoked      — explicit revocation flag
ReplacedByToken — audit trail: which token replaced this one
RevokedReason  — "Replaced" | "Manually revoked" | "SecurityReset"
IsActive       — computed: !IsRevoked && !IsExpired
```

### Token Lifecycle

```
Login  → JwtService.GenerateRefreshToken() → saved to DB → returned to client
       
Refresh → verify JWT signature (expired OK) → load RefreshToken from DB
        → assert IsActive → old.Revoke("Replaced", newToken)
        → JwtService.GenerateRefreshToken() → saved to DB → new tokens returned

Revoke / Logout → RevokeTokenCommand → token.Revoke("Manually revoked") → saved
```



```
1. POST /api/auth/login
   → Validate credentials against ASP.NET Identity
   → Verify AppUser belongs to tenant
   → Generate JWT (access token, 60 min)
   → Generate RefreshToken (stored in DB, 7 days)
   → Return tokens (access token also set as HttpOnly cookie)

2. Subsequent requests
   → Bearer token from Authorization header OR HttpOnly cookie
   → JwtBearerEvents reads cookie if header absent

3. POST /api/auth/refresh
   → Validate expired access token (signature only, ignore expiry)
   → Look up refresh token in DB — must be active
   → Revoke old refresh token, generate new one (rotation)
   → Return new access + refresh tokens

4. POST /api/auth/revoke / POST /api/auth/logout
   → Revoke stored refresh token
   → Clear access_token cookie
```

## CQRS Request Flow

```
Controller/PageModel
  → mediator.Send(Command/Query)
    → LoggingBehaviour (log entry)
    → ValidationBehaviour (FluentValidation, throws on failure)
    → CommandHandler / QueryHandler
      → Repository / Domain Entity
      → unitOfWork.SaveChangesAsync()
    → Return DTO
  → HTTP Response
```

## Appointment Lifecycle State Machine

```
                 ┌─────────┐
                 │ PENDING │
                 └────┬────┘
                      │ Confirm()
                 ┌────▼──────┐
                 │ CONFIRMED │
           ┌─────┴────┬──────┘
           │          │
    NoShow()│  Cancel()│  Complete()
     ┌──────▼─┐   ┌───▼────┐  ┌──────────┐
     │NO SHOW │   │CANCELLED│  │COMPLETED │
     └────────┘   └────────┘  └──────────┘
```

## Error Handling Strategy

All errors are handled by `ExceptionHandlingMiddleware`:

| Exception Type              | HTTP Status | Use Case                          |
|-----------------------------|-------------|-----------------------------------|
| `ValidationException`       | 400         | FluentValidation / business rules |
| `NotFoundException`         | 404         | Resource not found                |
| `ForbiddenException`        | 403         | Authorization failure             |
| `DomainException`           | 422         | Domain invariant violation        |
| `Exception` (unhandled)     | 500         | Unexpected server error           |

## Dependency Injection Wiring

```csharp
// Program.cs
builder.Services.AddApplication();      // MediatR, AutoMapper, FluentValidation
builder.Services.AddInfrastructure(configuration); // EF Core, Identity, JWT, Repositories
```

## Security Considerations

- JWT access tokens delivered via HttpOnly, Secure, SameSite=Strict cookies
- Refresh tokens stored hashed in SQLite with expiry + revocation tracking
- Token rotation on every refresh (old token marked replaced)
- Password requirements: 8+ chars, upper, lower, digit, special character
- All tenant-scoped operations validate tenant ownership
- Role-based authorization enforced at controller/page and handler level

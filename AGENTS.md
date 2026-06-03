# AGENTS.md — Copilot & AI Agent Guide

## Project Overview
AppointmentSaaS is a multi-tenant appointment management platform built with ASP.NET Core 9, Clean Architecture, EF Core (SQLite), ASP.NET Core Identity, and JWT authentication with refresh tokens.

## Architecture Summary
```
AppointmentSaaS.Domain          → Entities, Value Objects, Domain Events, Interfaces
AppointmentSaaS.Application     → CQRS Commands/Queries (MediatR), DTOs, Validators
AppointmentSaaS.Infrastructure  → EF Core DbContext, Repositories, Identity, JWT
AppointmentSaaS.Web             → Razor Pages + REST API Controllers, Middleware
AppointmentSaaS.Tests           → Unit & Integration Tests
```

## Domain Model Hierarchy

### Base Classes (`AppointmentSaaS.Domain/Common/`)
| Class | Extends | Adds |
|---|---|---|
| `BaseEntity` | — | `Id` (Guid), domain event collection |
| `AuditableEntity` | `BaseEntity` | `CreatedAt/By`, `UpdatedAt/By` via `IAuditableEntity` |
| `SoftDeleteEntity` | `AuditableEntity` | `IsDeleted`, `DeletedAt`, `DeletedBy`; `SoftDelete()` / `Restore()` |

### Entity Inheritance
| Entity | Base | Notes |
|---|---|---|
| `Tenant` | `SoftDeleteEntity` | Aggregate root; owns Businesses, Users, Staff, Services, Appointments, Customers |
| `Business` | `SoftDeleteEntity` | Location/branch under a Tenant |
| `AppUser` | `SoftDeleteEntity` | Identity-linked user; carries `UserRole` |
| `Staff` | `SoftDeleteEntity` | Staff member; optionally scoped to a Business |
| `Customer` | `SoftDeleteEntity` | Client/walk-in customer; optionally linked to AppUser |
| `Service` | `SoftDeleteEntity` | Bookable service; optionally scoped to a Business |
| `SubscriptionPlan` | `AuditableEntity` | Platform subscription tier with usage limits |
| `Appointment` | `AuditableEntity` | Booking aggregate root; references Staff, Service, Customer or AppUser |
| `RefreshToken` | `BaseEntity` | JWT refresh token; no audit needed |

### Relationships
```
SubscriptionPlan (1) ──< Tenant (M)
Tenant (1) ──< Business (M)
Tenant (1) ──< AppUser (M)
Tenant (1) ──< Staff (M)
Tenant (1) ──< Service (M)
Tenant (1) ──< Customer (M)
Tenant (1) ──< Appointment (M)

Business (1) ──< Staff (M)      [optional: staff may be tenant-wide]
Business (1) ──< Service (M)    [optional: service may be tenant-wide]
Business (1) ──< Appointment (M) [optional]

Staff (1) ──< Appointment (M)
Service (1) ──< Appointment (M)
Customer (1) ──< Appointment (M)  [walk-in / Customer-entity path]
AppUser (1) ──< Appointment (M)   [portal / AppUser path — legacy]
AppUser (1) ──< RefreshToken (M)
Customer (M) >── AppUser (1)      [optional link when customer self-registers]
```

## Key Patterns
- **Clean Architecture**: Dependency rule strictly flows inward (Web → Application → Domain)
- **CQRS**: All write operations use Commands, reads use Queries, dispatched via MediatR
- **Repository Pattern**: `IRepository<T>` generic + specialized repositories (`IAppointmentRepository`, `ITenantRepository`)
- **Unit of Work**: `IUnitOfWork` wraps EF Core transactions; always call `SaveChangesAsync` after mutations
- **Domain Events**: Raised inside domain entities, dispatched in infrastructure
- **Multi-Tenancy**: Tenant isolation via `TenantId` on all data-scoped entities; resolved from JWT claims
- **Soft Delete**: Entities extending `SoftDeleteEntity` use `SoftDelete()` instead of physical removal; EF global query filters exclude `IsDeleted = true` rows

## When Adding New Features
1. Add domain entity/enum changes in `AppointmentSaaS.Domain`
2. Choose the right base class: `SoftDeleteEntity` for most entities; `AuditableEntity` for aggregates that use status-based lifecycle (e.g., Appointment); `BaseEntity` for infrastructure entities (e.g., RefreshToken)
3. Add new Command or Query in appropriate `Features/` folder in `AppointmentSaaS.Application`
4. Add FluentValidation validator alongside the handler
5. Register new services in `DependencyInjection.cs` of the relevant project
6. Add EF Core configuration in `AppointmentSaaS.Infrastructure/Data/Configurations/`
   - Apply `HasQueryFilter(e => !e.IsDeleted)` for any `SoftDeleteEntity`
7. Create/update migration: `dotnet ef migrations add <Name> --project AppointmentSaaS.Infrastructure --startup-project AppointmentSaaS.Web`
8. Add Razor Page or API controller in `AppointmentSaaS.Web`
9. Write tests in `AppointmentSaaS.Tests`

## Critical Invariants (Do Not Violate)
- Domain entities MUST use private constructors + static factory methods
- All data access MUST go through Repository interfaces, never directly via DbContext in Application
- Always check for appointment conflicts via `IAppointmentRepository.HasConflictAsync` before creating
- JWT tokens read from `HttpOnly` cookies; refresh tokens stored in DB with expiry
- **Token rotation**: on every `/api/auth/refresh`, old refresh token is revoked (`"Replaced"`) and a new one issued — both persisted via `IRepository<RefreshToken>` + `IUnitOfWork`
- **Revocation**: call `RevokeTokenCommand` to invalidate any active refresh token; `RefreshToken.IsActive` (not revoked AND not expired) guards all token use
- Tenant context MUST be validated in all Application handlers that write data
- NEVER physically delete `SoftDeleteEntity` rows — call `SoftDelete()` instead
- `Appointment` takes either `ClientId` (AppUser) or `CustomerId` (Customer) — validate that exactly one is set

## Running the Project
```bash
cd AppointmentSaaS.Web
dotnet ef database update
dotnet run
```

## Running Tests
```bash
dotnet test AppointmentSaaS.Tests
```

## Adding EF Core Migrations
```bash
dotnet ef migrations add <MigrationName> \
  --project AppointmentSaaS.Infrastructure \
  --startup-project AppointmentSaaS.Web
dotnet ef database update \
  --project AppointmentSaaS.Infrastructure \
  --startup-project AppointmentSaaS.Web
```

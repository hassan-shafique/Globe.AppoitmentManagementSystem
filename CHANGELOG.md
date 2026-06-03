# Changelog

All notable changes to Globe Appointment Management System are documented here.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/).

---

## [1.1.0] — 2026-06-03

### Added

#### Domain Layer — Core Domain Model
- `AuditableEntity` abstract class implementing `IAuditableEntity`: consolidates `CreatedAt/By` and `UpdatedAt/By` properties so entities no longer repeat boilerplate audit fields
- `SoftDeleteEntity` abstract class extending `AuditableEntity`: adds `IsDeleted`, `DeletedAt`, `DeletedBy` with `SoftDelete(deletedBy?)` and `Restore()` methods; physical row deletion is no longer needed for any entity in the hierarchy
- `Business` entity (`SoftDeleteEntity`): represents a physical or logical location/branch within a `Tenant`; carries `Name`, `Address`, `City`, `Phone`, `Email`, and `IsActive`; owned by a Tenant and optionally associated with `Staff`, `Service`, and `Appointment`
- `Customer` entity (`SoftDeleteEntity`): dedicated client/walk-in entity scoped to a Tenant; supports optional link to `AppUser` via `LinkToAppUser()`; enables bookings without a portal account; exposes `FullName`, `Notes` (staff-only), and `IsActive`
- `SubscriptionPlan` entity (`AuditableEntity`): defines a billing tier (name, monthly/annual price, optional usage limits for staff, appointments/month, and business locations); referenced by `Tenant.SubscriptionPlanId`
- `Appointment.CreateForCustomer()` factory method: creates an appointment linked to a `Customer` record rather than an `AppUser` (walk-in / admin-created bookings)
- `Appointment.CustomerId` and `Appointment.BusinessId` optional foreign keys
- `Appointment.Customer` and `Appointment.Business` navigation properties
- `Staff.BusinessId` optional FK and `AssignToBusiness()` method
- `Staff.Update()` method for profile field mutation
- `Service.BusinessId` optional FK
- `Tenant.SubscriptionPlanId` FK, `Tenant.SetSubscriptionPlan()` method, `Tenant.Businesses` and `Tenant.Customers` navigation collections

#### Domain Layer — Refactoring
- `Tenant`, `AppUser`, `Staff`, `Customer`, `Service`, `Business` now extend `SoftDeleteEntity` (previously `BaseEntity + IAuditableEntity` manual duplication)
- `Appointment`, `SubscriptionPlan` extend `AuditableEntity`
- All domain entities and base classes annotated with comprehensive XML `<summary>` and `<param>` documentation comments

### Changed
- `Service.Create()` accepts an optional `businessId` parameter (non-breaking; defaults to `null`)
- `Staff.Create()` accepts an optional `businessId` parameter (non-breaking; defaults to `null`)
- `Appointment.Create()` accepts an optional `businessId` parameter (non-breaking; defaults to `null`)
- `Appointment.ClientId` changed from `Guid` to `Guid?` — accommodates the Customer booking path where no `AppUser` is involved

### Architecture
- Updated `Architecture.md` with full base class hierarchy diagram, soft delete strategy section, and expanded relationship graph
- Updated `AGENTS.md` with entity inheritance table, relationship map, soft delete invariant, and guidance on choosing the correct base class

---

## [1.0.0] — 2026-06-03

### Added

#### Architecture & Infrastructure
- Clean Architecture solution with Domain, Application, Infrastructure, Web, and Tests projects
- Entity Framework Core 9 with SQLite provider and code-first migrations
- ASP.NET Core Identity integration with role-based authorization
- JWT authentication with HttpOnly cookie delivery and refresh token rotation
- Serilog structured logging with console and rolling file sinks
- MediatR CQRS pipeline with logging and validation behaviours
- AutoMapper profile for domain entity to DTO projection
- FluentValidation for request validation in MediatR pipeline
- Generic `Repository<T>` pattern with specialized repositories
- `UnitOfWork` wrapping EF Core transactions
- `DataSeeder` for initial tenant and admin user seeding

#### Domain Layer
- `Tenant` aggregate with slug-based identification and activation control
- `AppUser` entity linking ASP.NET Identity to tenant context
- `Appointment` aggregate with full lifecycle state machine (Pending → Confirmed → Completed/Cancelled/NoShow)
- `Service` entity with duration and pricing
- `Staff` entity with tenant association
- `RefreshToken` entity with expiry, revocation, and rotation tracking
- `AppointmentStatus` and `UserRole` enumerations
- Domain events: `AppointmentCreatedEvent`, `AppointmentCancelledEvent`
- `DomainException` and `AppointmentConflictException` for domain rule violations
- `BaseEntity` with domain event collection
- `IAuditableEntity` for automatic audit timestamps
- `ValueObject` base class with structural equality

#### Application Layer
- Login, Register, RefreshToken, RevokeToken command handlers
- Create, Cancel, Confirm, Complete appointment command handlers
- GetAppointmentById, GetAppointmentsByTenant query handlers
- CreateTenant, GetTenantBySlug handlers
- CreateService, GetServicesByTenant handlers
- `ITokenService`, `ICurrentUserService`, `IEmailService`, `ITenantService` interfaces
- `ValidationBehaviour` MediatR pipeline behaviour
- `LoggingBehaviour` MediatR pipeline behaviour
- Application exception types: `ValidationException`, `NotFoundException`, `ForbiddenException`

#### Web Layer
- Razor Pages: Home, Login, Register, Dashboard, Appointments (Index, Create, Details)
- REST API controllers: Auth, Appointments, Tenants, Services
- `ExceptionHandlingMiddleware` with structured JSON error responses
- Bootstrap 5 responsive layout with navigation
- Antiforgery token protection on Razor Pages

#### Tests
- Domain unit tests for Appointment, Tenant, RefreshToken entities
- Application unit tests for CreateAppointmentCommandHandler, LoginCommandHandler
- Infrastructure integration tests for AppointmentRepository using EF InMemory

---

## [Unreleased]

### Planned
- EF Core configurations and migration for Business, Customer, SubscriptionPlan entities
- EF Core global query filters for soft delete on all `SoftDeleteEntity` types
- Application CQRS handlers for Business, Customer, SubscriptionPlan
- Staff availability scheduling
- Email notification integration (SMTP)
- Swagger/OpenAPI documentation
- Docker containerization
- Recurring appointment support
- Calendar view (FullCalendar.js integration)
- Tenant admin portal
- Reporting and analytics dashboard

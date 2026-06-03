# Globe Appointment Management System

A production-ready multi-tenant Appointment Management SaaS built with ASP.NET Core 9, Clean Architecture, and modern .NET best practices.

## Features

- **Multi-Tenant Architecture** — Fully isolated tenant environments with slug-based routing
- **JWT Authentication** — Stateless access tokens with HttpOnly cookie delivery
- **Refresh Tokens** — Secure token rotation with revocation support
- **CQRS Pattern** — MediatR-based Commands and Queries for clean separation
- **Clean Architecture** — Strict dependency inversion across all layers
- **Razor Pages UI** — Server-side rendered frontend with Bootstrap 5
- **REST API** — Full API surface for headless integration
- **EF Core + SQLite** — Code-first migrations with full audit trail
- **ASP.NET Core Identity** — Role-based authorization (SuperAdmin, TenantAdmin, Staff, Client)
- **Domain Events** — Decoupled side effects (email notifications, audit logs)
- **FluentValidation** — Pipeline validation with descriptive error messages
- **AutoMapper** — Clean entity-to-DTO projection
- **Serilog** — Structured logging with file rolling

## Tech Stack

| Layer          | Technology                                      |
|----------------|-------------------------------------------------|
| Web            | ASP.NET Core 9, Razor Pages, REST API           |
| Application    | MediatR, AutoMapper, FluentValidation           |
| Domain         | Clean Domain Model, Domain Events               |
| Infrastructure | EF Core 9, SQLite, ASP.NET Core Identity, JWT   |
| Tests          | xUnit, Moq, FluentAssertions, EF InMemory       |
| Logging        | Serilog                                         |

## Solution Structure

```
AppointmentSaaS/
├── AppointmentSaaS.Domain/          # Entities, Enums, Events, Interfaces
│   ├── Common/                      # BaseEntity, ValueObject, IAuditableEntity
│   ├── Entities/                    # Tenant, AppUser, Appointment, Service, Staff, RefreshToken
│   ├── Enums/                       # AppointmentStatus, UserRole
│   ├── Events/                      # DomainEvent, AppointmentCreatedEvent, AppointmentCancelledEvent
│   ├── Exceptions/                  # DomainException, AppointmentConflictException
│   └── Interfaces/                  # IRepository<T>, IUnitOfWork, IAppointmentRepository, ITenantRepository
│
├── AppointmentSaaS.Application/     # Use Cases (CQRS)
│   ├── Common/                      # Behaviours, Exceptions, Interfaces, Mappings
│   ├── DTOs/                        # Auth, Appointments, Tenants, Services, Staff
│   └── Features/                    # Auth, Appointments, Tenants, Services Commands & Queries
│
├── AppointmentSaaS.Infrastructure/  # Data Access & External Services
│   ├── Data/                        # AppDbContext, EF Configurations, Seeder
│   ├── Identity/                    # ApplicationIdentityUser
│   ├── Repositories/                # Repository<T>, AppointmentRepository, TenantRepository, UnitOfWork
│   └── Services/                    # TokenService, CurrentUserService, EmailService
│
├── AppointmentSaaS.Web/             # Presentation Layer
│   ├── Controllers/Api/             # AuthController, AppointmentsController, TenantsController, ServicesController
│   ├── Pages/                       # Razor Pages (Index, Auth, Dashboard, Appointments)
│   ├── Middleware/                  # ExceptionHandlingMiddleware
│   └── Extensions/                  # WebApplicationExtensions
│
└── AppointmentSaaS.Tests/           # Test Suite
    ├── Domain/                      # Entity behavior tests
    ├── Application/                 # Handler unit tests
    └── Infrastructure/              # Repository integration tests
```

## Quick Start

### Prerequisites
- .NET 9 SDK
- Git

### Setup

```bash
git clone <repo-url>
cd Globe.AppoitmentManagementSystem

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update --project AppointmentSaaS.Infrastructure --startup-project AppointmentSaaS.Web

# Run the application
dotnet run --project AppointmentSaaS.Web
```

Navigate to `https://localhost:5001`

### Default Credentials (Development)
- **Organization:** `globe`
- **Email:** `admin@globe.com`
- **Password:** `Admin@123!`

## API Endpoints

### Authentication
| Method | Endpoint           | Description          | Auth |
|--------|--------------------|----------------------|------|
| POST   | /api/auth/login    | Login, get JWT       | No   |
| POST   | /api/auth/register | Register new client  | No   |
| POST   | /api/auth/refresh  | Rotate refresh token | No   |
| POST   | /api/auth/revoke   | Revoke refresh token | Yes  |
| POST   | /api/auth/logout   | Clear auth cookie    | Yes  |

### Appointments
| Method | Endpoint                              | Description              | Auth             |
|--------|---------------------------------------|--------------------------|------------------|
| GET    | /api/appointments/tenant/{tenantId}   | List tenant appointments | Yes              |
| GET    | /api/appointments/{id}                | Get appointment          | Yes              |
| POST   | /api/appointments                     | Create appointment       | Yes              |
| PATCH  | /api/appointments/{id}/confirm        | Confirm appointment      | TenantAdmin/Staff|
| PATCH  | /api/appointments/{id}/cancel         | Cancel appointment       | Yes              |
| PATCH  | /api/appointments/{id}/complete       | Complete appointment     | TenantAdmin/Staff|

## Configuration

Key settings in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=AppointmentSaaS.db"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyHere",
    "Issuer": "AppointmentSaaS",
    "Audience": "AppointmentSaaS",
    "ExpiryMinutes": "60"
  }
}
```

> **Security:** Never commit real secret keys. Use environment variables or Azure Key Vault in production.

## Running Tests

```bash
dotnet test
# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development workflow and coding standards.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history.

## License

MIT License — see LICENSE file for details.

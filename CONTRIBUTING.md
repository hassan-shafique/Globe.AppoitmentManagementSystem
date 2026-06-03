# Contributing to Globe Appointment Management System

Thank you for contributing! Please follow these guidelines to keep the codebase consistent and maintainable.

## Development Setup

```bash
git clone <repo-url>
cd Globe.AppoitmentManagementSystem
dotnet restore
dotnet build
dotnet test
```

## Branching Strategy

| Branch Pattern      | Purpose                          |
|---------------------|----------------------------------|
| `main`              | Production-ready code            |
| `develop`           | Integration branch               |
| `feature/<name>`    | New features                     |
| `fix/<name>`        | Bug fixes                        |
| `refactor/<name>`   | Refactoring without behavior change |

## Commit Message Format

```
<type>(<scope>): <short description>

<optional body>

<optional footer>
```

**Types:** `feat`, `fix`, `refactor`, `test`, `docs`, `chore`

**Examples:**
```
feat(appointments): add recurring appointment support
fix(auth): prevent refresh token reuse after revocation
test(domain): add appointment overlap edge case tests
```

## Adding a New Feature

1. **Domain first** — model the entity or value object in `AppointmentSaaS.Domain`
2. **Application second** — add Command or Query with handler and FluentValidation validator
3. **Infrastructure** — add EF configuration and repository methods if needed
4. **Migration** — `dotnet ef migrations add <Name> --project AppointmentSaaS.Infrastructure --startup-project AppointmentSaaS.Web`
5. **Web** — add Razor Page and/or API controller action
6. **Tests** — write unit tests for domain behavior, handler logic, and repository queries

## Code Style

- Use C# 13 / .NET 9 language features
- Primary constructors for dependency injection
- Records for DTOs and Commands/Queries
- `file`-scoped namespaces
- Private constructors + static factory methods on domain entities
- No logic in controllers — delegate to MediatR
- No direct DbContext access in Application layer

## Pull Request Requirements

- [ ] All existing tests pass (`dotnet test`)
- [ ] New behavior is covered by tests
- [ ] No compiler warnings introduced
- [ ] Migration added if schema changed
- [ ] `CHANGELOG.md` updated with a brief description of changes

## Security Guidelines

- Never log PII (emails, phone numbers) at `Information` level or above
- All inputs validated via FluentValidation before reaching handlers
- Never expose internal IDs in URLs without authorization checks
- Refresh tokens must be revoked on logout and on rotation
- JWT secret key must come from environment/secret manager — not appsettings in production

## Questions?

Open an issue or start a discussion in the repository.

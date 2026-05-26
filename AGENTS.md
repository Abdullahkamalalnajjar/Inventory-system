# Inventory Management System — Agent Guide

This document describes the architecture, conventions, and workflows for the **Inventory Management System** .NET solution. Read this first before making any code changes.

---

## Project Overview

This is a .NET 8 Web API for an inventory management backend. It tracks products, warehouses, stock levels, stock movements, purchase invoices, and sales invoices. It also includes identity management (users, roles, JWT auth) and a Paymob payment integration.

Key business rule:
> **Stock quantities never change arbitrarily.** Every quantity change must produce a `StockMovement` record.

A detailed domain guide exists at `InventoryDomainGuide.md` (Arabic) explaining how Products, Warehouses, StockItems, StockMovements, PurchaseInvoices, and SalesInvoices relate.

---

## Technology Stack

- **Runtime / Framework:** .NET 8 (ASP.NET Core Web API)
- **Language:** C# with `Nullable=enable` and `ImplicitUsings=enable`
- **Database:** SQL Server (EF Core 8)
- **ORM:** Entity Framework Core 8 with code-first migrations
- **Architecture:** Clean Architecture (Domain → Application → Infrastructure → Api)
- **CQRS / Mediator:** MediatR 14.x
- **Validation:** FluentValidation (via MediatR pipeline behavior)
- **Caching:** `Microsoft.Extensions.Caching.Hybrid` (10.4.0) with MediatR pipeline caching
- **Identity:** ASP.NET Core Identity with custom `AppUser` extending `IdentityUser`
- **Authentication:** JWT Bearer tokens with refresh-token rotation
- **Authorization:** Role-based + Permission-claim-based policies
- **API Versioning:** Asp.Versioning.Mvc 8.1.0
- **Documentation:** Swashbuckle.AspNetCore 6.6.2 (Swagger / OpenAPI)
- **Payment Provider:** Paymob (Egypt) — supports both Intention API and iframe flows
- **Notifications:** SMTP (optional, configurable)

---

## Solution Structure

```
InventorySystem.sln
├── InventoryManagementSystem.Domain           # No external dependencies
├── InventoryManagementSystem.Application        # References Domain
├── InventoryManagementSystem.Infrastructure     # References Domain + Application
└── InventoryManagementSystem.Api                # References Infrastructure
```

### Domain (`InventoryManagementSystem.Domain`)

Pure domain logic with no framework dependencies.

| Folder | Purpose |
|--------|---------|
| `Common/` | `Entity`, `AuditableEntity`, `DomainEvent`, Result/Error pattern |
| `Product/` | `Product`, `Category`, `Unit`, and error classes |
| `Warehouse/` | `Warehouse` and error classes |
| `Stock/` | `StockItem`, `StockMovement`, `StockMovementType`, errors |
| `Invoices/` | `PurchaseInvoice`, `SalesInvoice`, their items, `InvoiceStatus`, errors |
| `Identity/` | `Role` enum, `RefreshToken` |

Key patterns:
- Entities use **Guid** primary keys.
- Entities inherit `Entity` (has `Id` + `DomainEvents`) or `AuditableEntity` (adds `CreatedAtUtc`, `CreatedBy`, `LastModifiedUtc`, `LastModifiedBy`).
- Domain methods return `Result<T>` or `Result<Updated>` / `Result<Deleted>` instead of throwing exceptions for business-rule failures.
- Static factory methods (e.g., `Category.Create(...)`) encapsulate creation rules.
- Errors are defined in static error classes (e.g., `CategoryErrors`, `ProductErrors`, `StockErrors`).

### Application (`InventoryManagementSystem.Application`)

Use-case layer. Contains CQRS handlers, DTOs, mappers, validators, and behaviors.

| Folder | Purpose |
|--------|---------|
| `Common/Behaviours/` | MediatR pipeline behaviors: `ValidationBehavior`, `CachingBehavior`, `LoggingBehaviour`, `PerformanceBehaviour`, `UnhandledExceptionBehaviour` |
| `Common/Interfaces/` | Abstractions: `IApplicationDbContext`, `IIdentityService`, `ITokenProvider`, `INotificationService`, `IPaymentCheckoutService`, `IUser`, `ICachedQuery`, etc. |
| `Common/Models/` | Request/response models for identity and payments |
| `Common/Security/` | `Permissions`, `AuthorizationPolicies`, `PermissionClaimTypes` |
| `Features/{Feature}/` | One folder per feature (e.g., `Features/Categories/`) with subfolders: `Commands/`, `Queries/`, `Dtos/`, `Mappers/` |

Each command/query follows this structure:
- `{Name}Command.cs` — `IRequest<Result<T>>` record
- `{Name}CommandHandler.cs` — handler class
- `{Name}CommandValidator.cs` — FluentValidation validator
- `Dtos/{Name}Dto.cs` — output DTO
- `Mappers/{Name}Mapper.cs` — static mapper class

### Infrastructure (`InventoryManagementSystem.Infrastructure`)

Implements interfaces defined in Application.

| Folder | Purpose |
|--------|---------|
| `Data/` | `AppDbContext`, EF configurations, interceptors, migrations, seeding |
| `Identity/` | `AppUser`, `IdentityService`, `TokenProvider`, `IdentityClaimsFactory` |
| `Notifications/` | `SmtpNotificationService`, `SmtpOptions` |
| `Payments/` | `PaymobCheckoutService`, `PaymobWebhookService`, `PaymobOptions` |

### Api (`InventoryManagementSystem.Api`)

ASP.NET Core host. Thin controllers that delegate to MediatR.

| Folder | Purpose |
|--------|---------|
| `Controllers/` | API controllers inheriting from `ApiController` |
| `Infrastructure/` | `GlobalExceptionHandler`, `RequestLogContextMiddleware` |
| `Extensions/` | `ProblemExtensions` — maps `Result` errors to HTTP status codes |
| `Requests/` | Request contract records used by controllers |
| `Services/` | `CurrentUser` implementing `IUser` |

---

## Build and Run Commands

### Prerequisites
- .NET 8 SDK
- SQL Server instance (or update `appsettings.json` connection string to use LocalDB/SQLite for local dev)

### Build the solution
```bash
dotnet build InventorySystem.sln
```

### Run the API
```bash
cd InventoryManagementSystem.Api
dotnet run
```

By default the API runs on:
- HTTP: `http://localhost:5002`
- HTTPS: `https://localhost:5003`

Swagger UI opens automatically at `/swagger` in Development.

### Apply database changes
The app uses `EnsureCreatedAsync()` at startup, which is suitable for prototyping. For production-like scenarios, use EF Migrations:

```bash
cd InventoryManagementSystem.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../InventoryManagementSystem.Api
dotnet ef database update --startup-project ../InventoryManagementSystem.Api
```

Note: there are two migration folders — `Data/Migrations/` and `Migrations/` at the Infrastructure root. The one actively used by `AppDbContext` is under `Data/Migrations/` (configured via `AppDbContextFactory` or convention).

---

## Code Style & Conventions

- **Language:** C# 12 features are used (primary constructors, collection expressions, etc.).
- **Nullability:** All projects have `<Nullable>enable</Nullable>`. Avoid null-forgiving operator (`!`) unless truly unavoidable.
- **Usings:** `ImplicitUsings` is enabled. Do not add redundant `using System;` etc.
- **Records:** Use `record` for MediatR requests/responses and DTOs. Use `sealed` classes for handlers and domain entities.
- **Primary constructors:** Heavily used in handlers and infrastructure services.
- **Naming:**
  - Async methods end with `Async`.
  - Private fields in primary constructors are assigned to `private readonly` fields with underscore prefix.
  - Constants use PascalCase.
- **Error handling:** Do **not** throw exceptions for business validation. Return `Error.Validation(...)`, `Error.NotFound(...)`, `Error.Conflict(...)`, etc., and propagate them through `Result<T>`.
- **Controller pattern:** Controllers call `_sender.Send(...)` and use `result.Match(onValue: ..., onError: Problem)` to return responses. The base `ApiController.Problem(List<Error>)` extension maps errors to correct HTTP status codes.

---

## Feature Development Patterns

When adding a new feature (e.g., Products, Warehouses, Invoices), follow the existing **Categories** feature as the canonical example.

### Folder layout
```
Features/{PluralName}/
  Dtos/
  Mappers/
  Commands/
    Create{Name}/
      Create{Name}Command.cs
      Create{Name}CommandHandler.cs
      Create{Name}CommandValidator.cs
    Update{Name}/
    Delete{Name}/
  Queries/
    Get{PluralName}/
    Get{Name}ById/
```

### Command/Query conventions
1. **Validate** with FluentValidation (registered automatically via MediatR `ValidationBehavior`).
2. **Load** aggregate via `IApplicationDbContext`.
3. **Check** domain rules (duplicates, missing records) in the handler.
4. **Call** domain methods on the aggregate.
5. **Save** changes with `_context.SaveChangesAsync(cancellationToken)`.
6. **Return** a domain result (`Result<T>`, `Result<Updated>`, `Result<Deleted>`).

### Caching queries
If a query should be cached, implement `ICachedQuery<T>`:
```csharp
public sealed record GetCategoriesQuery() : ICachedQuery<Result<List<CategoryDto>>>
{
    public string CacheKey => CategoriesCacheKeys.CategoriesListKey;
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public IEnumerable<string>? Tags => [CategoriesCacheKeys.CategoriesTag];
}
```
Invalidate cache tags on write operations using `HybridCache.RemoveByTagAsync(...)`.

---

## Testing

There are **no test projects** in the solution yet. If you add tests, create an `xUnit` or `NUnit` project following standard .NET conventions. Keep tests in a separate project (e.g., `InventoryManagementSystem.Tests`) that references the Application and/or Domain projects.

Recommended testing approach for this codebase:
- **Unit tests** for domain logic (e.g., `StockItem.AddQuantity`, `Category.Create`).
- **Integration tests** for MediatR handlers using an in-memory EF Core provider or Testcontainers SQL Server.

---

## Security

### Authentication
- JWT Bearer tokens. Secret, Issuer, Audience, and expiration are configured in `JwtSettings`.
- Refresh tokens are stored in the database (`RefreshToken` entity) and rotated on every token generation.
- Soft-deleted users are blocked at JWT validation time (`OnTokenValidated`) with a custom `X-Force-Logout` header.

### Authorization
- Policies are defined in `AuthorizationPolicies` and registered in `DependencyInjection.cs`.
- Permissions are claim-based (`PermissionClaimTypes.Permission`).
- Roles: `Manager` (full permissions) and `Member` (self-read + payments).
- Permission-to-role mapping is centralized in `Permissions.GetForRole(string role)`.

### Sensitive configuration
- `appsettings.json` contains production connection strings and Paymob API keys in the committed file. **For production, move secrets to environment variables or a secrets manager.**
- Default admin credentials are seeded from `SeedData` configuration.

---

## Configuration

Key sections in `appsettings.json`:

| Section | Purpose |
|---------|---------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `JwtSettings` | JWT secret, issuer, audience, expiration |
| `Cors:AllowedOrigins` | Additional allowed origins beyond localhost |
| `Paymob` | Paymob API base URL, keys, iframe ID, currency, webhooks |
| `Smtp` | Optional SMTP settings for email notifications |
| `SeedData` | Toggle seeding and default admin account details |

---

## Database & Migrations

- `AppDbContext` extends `IdentityDbContext<AppUser, IdentityRole, string>`.
- All entity configurations are in `Data/Configurations/` and applied via `modelBuilder.ApplyConfigurationsFromAssembly(...)`.
- The `AuditableEntityInterceptor` automatically stamps `CreatedAtUtc`, `CreatedBy`, `LastModifiedUtc`, `LastModifiedBy` on save.
- On startup, `EnsureCreatedAsync()` is called, followed by `AppDbContextSeeder.SeedAsync()` to create roles and an admin user.

### Adding a migration
```bash
cd InventoryManagementSystem.Infrastructure
dotnet ef migrations add YourMigrationName --startup-project ../InventoryManagementSystem.Api
```

---

## Payment Integration (Paymob)

The `PaymentsController` and `PaymobController` expose checkout and webhook endpoints. The `PaymobCheckoutService` supports two modes:
1. **Intention API** (modern) — when `IframeId` is empty.
2. **Iframe flow** (legacy) — when `IframeId` is configured.

Webhook signature verification is handled in `PaymobWebhookService`.

---

## API Versioning

The API uses URL-path versioning: `/api/v1/categories`.
- Controllers decorate with `[ApiVersion("1.0")]` and `[Route("api/v{version:apiVersion}/...")]`.
- Swagger groups endpoints by version (`v1`).

---

## Important Notes for Agents

1. **Do not assume general patterns.** Always follow the existing feature structure (Categories is the reference).
2. **Never throw exceptions for business rules.** Return `Error.*` and wrap in `Result<T>`.
3. **Keep domain logic in Domain.** Application handlers orchestrate; Domain entities enforce invariants.
4. **Use `IApplicationDbContext`** instead of referencing `AppDbContext` directly from Application.
5. **Add validators** for every command/query that accepts user input.
6. **Use `CancellationToken`** in all async handler methods.
7. **When adding new entities**, consider whether they need `AuditableEntity` vs `Entity`.
8. **Permissions:** If a new endpoint requires authorization, add a permission constant in `Permissions`, reference it in `Permissions.GetForRole`, and create a policy in `AuthorizationPolicies`.
9. **Language:** Business/domain documentation may be in Arabic (`InventoryDomainGuide.md`), but all code, comments, and `AGENTS.md` are in English.

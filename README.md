# MyShopServer

Backend API for a small shop management system (products, categories, orders, promotions, KPI/commission, reporting). The API is exposed via GraphQL and secured with JWT.

## Tech Stack

- **.NET**: .NET8
- **API**: GraphQL (HotChocolate)
- **ORM**: Entity Framework Core (SQLite provider)
- **Database**: SQLite (`MyShop.db`)
- **Auth**: JWT Bearer Authentication + Role-based Authorization
- **Media**: Cloudinary (image upload/storage)

## Key Features

- **Auth**: login, JWT token issuance, role claims
- **Users & Roles**: user management with role filtering (Admin/Moderator scoped)
- **Catalog**: categories, products, product images
- **Promotions**:
 - Product / Category promotions auto-applied to order item unit prices
 - Order promotions manually selected (Order scope only)
 - "Best" discount selection by highest percentage
- **Orders**:
 - create/update/delete, order detail
 - price calculation with discount application and overflow-safe arithmetic
- **KPI / Commission**:
 - monthly targets, tier rules, dashboard estimate, monthly calculation persistence
- **Reporting**:
 - revenue & profit series
 - top-selling products

## Architecture

A pragmatic layered structure:

- `Application/GraphQL`: GraphQL queries/mutations and inputs
- `Application/Services`: business logic (service layer)
- `Domain`: entities and enums
- `Infrastructure/Data`: EF Core `DbContext`, configurations, migrations, seed
- `Infrastructure/Cloudinary`: Cloudinary integration
- `DTOs`: request/response DTOs shared across layers

## Project Structure

```
MyShopServer/
├─ Application/
│ ├─ Common/
│ ├─ GraphQL/
│ └─ Services/
├─ Domain/
├─ DTOs/
├─ Infrastructure/
│ ├─ Cloudinary/
│ └─ Data/
├─ Migrations/
├─ Program.cs
└─ MyShopServer.csproj
```

## Database

- Provider: **SQLite**
- Default connection string (development): `Data Source=MyShop.db`
- EF Core migrations are applied automatically on startup.
- Seed data is applied on startup (first run).

## Authentication & Authorization

- JWT bearer tokens are required for most GraphQL operations.
- Roles are enforced via HotChocolate `[Authorize(Roles = ...)]`.
- Default roles (seed): `Admin`, `Moderator`, `Sale`.

## Configuration / Environment Variables

Configuration is read from `appsettings.json` / `appsettings.Development.json`.

### Required

- `ConnectionStrings:DefaultConnection`
 - Example: `Data Source=MyShop.db`

- `Jwt:Key`
 - Secret key used to sign JWT tokens.

### Optional (only required for image upload)

- `Cloudinary:CloudName`
- `Cloudinary:ApiKey`
- `Cloudinary:ApiSecret`
- `Cloudinary:Folder` (default: `myshop`)

> Do not commit real secrets. Use user secrets or environment-specific config.

## Installation

Prerequisites:
- .NET SDK8

Restore dependencies:

- `dotnet restore`

## Run

Start the API:

- `dotnet run`

By default, GraphQL is served at:
- `https://localhost:5135/graphql`

> Ports may differ depending on your local launch settings.

## API Overview (GraphQL)

All operations are exposed via a single endpoint: `POST /graphql`.

### Common workflows

1) **Login**
- Mutation: `login(input: { username, password })`
- Returns: JWT token + user info

2) **Authenticated requests**
- Add header: `Authorization: Bearer <token>`

### Main domains

- **Auth**: `login`
- **Users**: `users`, `userById`, `createUser`, `updateUser`, `setUserActive`, `resetUserPassword`
- **Products/Categories**: list/detail + CRUD
- **Promotions**: list/detail + CRUD
- **Orders**: list/detail + create/update/delete
- **KPI**: tiers/targets/dashboard/calculate + queries
- **Reports**: revenue/profit series, product sales series

### Test requests

See `GRAPHQL_TEST_QUERIES.http` for ready-to-run examples.

## Additional Documentation

- `DOCS_PROMOTION_ORDER_SYSTEM.md` — promotion & order pricing rules
- `DOCS_KPI_SYSTEM.md` — KPI/commission rules and usage

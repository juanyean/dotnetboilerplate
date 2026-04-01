# MyDotNetApp — ASP.NET Core 10 Boilerplate

A Clean Architecture boilerplate with ASP.NET Core 10, Blazor Web App (Server Interactive), Minimal APIs, SignalR, and full CRUD sample.

---

## Prerequisites — Install These First

| Tool | Version Required | Download |
|---|---|---|
| .NET SDK | **10.0+** | https://dotnet.microsoft.com/download/dotnet/10.0 |
| EF Core CLI | Latest | `dotnet tool install -g dotnet-ef` |
| VS Code or Visual Studio 2022+ | Latest | https://code.visualstudio.com |

> ⚠️ **Important:** You must install the .NET 10 SDK. Run `dotnet --version` to verify it shows `10.x.x`.

---

## Quick Start

```bash
# 1. Navigate to the project
cd c:\Boilerplate

# 2. Restore packages
dotnet restore

# 3. Install EF Core CLI (if not done)
dotnet tool install -g dotnet-ef

# 4. Create database and apply migrations
cd src\MyDotNetApp.Web
dotnet ef database update

# 5. Run the application
dotnet run

# App available at: https://localhost:5001
```

### Default Admin Login
- **Email:** `admin@app.com`
- **Password:** `Admin@123`

---

## Project Structure

```
MyDotNetApp/
├── MyDotNetApp.sln
├── src/
│   ├── MyDotNetApp.Domain/          # Entities, Interfaces, Common types
│   ├── MyDotNetApp.Application/     # DTOs, Services, Mappings
│   ├── MyDotNetApp.Infrastructure/  # EF Core, Identity, JWT, SignalR, Serilog
│   └── MyDotNetApp.Web/             # Blazor UI + Minimal API endpoints
└── tests/
    ├── MyDotNetApp.Application.Tests/
    └── MyDotNetApp.Infrastructure.Tests/
```

---

## Technology Stack

| Concern | Technology |
|---|---|
| Framework | ASP.NET Core 10.0 |
| UI | Blazor Web App (Interactive Server) |
| UI Components | MudBlazor |
| Database | SQLite + EF Core |
| Authentication | ASP.NET Identity + JWT |
| Logging | Serilog (Console + Rolling File) |
| Real-Time | SignalR |
| Testing | xUnit + Moq + FluentAssertions |
| Mapping | AutoMapper |

---

## Pages

| Route | Description | Access |
|---|---|---|
| `/auth/login` | Login page | Anonymous |
| `/auth/register` | Register new account | Anonymous |
| `/` | Home dashboard | Authenticated |
| `/products` | Product list with search, add, edit, delete | Authenticated |
| `/admin/users` | User management | Admin only |
| `/admin/logs` | Application log viewer | Admin only |

---

## API Endpoints

| Method | URL | Description | Auth |
|---|---|---|---|
| POST | `/api/auth/login` | Login, returns JWT | Anonymous |
| POST | `/api/auth/register` | Register user | Anonymous |
| GET | `/api/products` | Search products (paginated) | Required |
| GET | `/api/products/{id}` | Get product by ID | Required |
| POST | `/api/products` | Create product | Required |
| PUT | `/api/products/{id}` | Update product | Required |
| DELETE | `/api/products/{id}` | Soft-delete product | Required |
| GET | `/api/users` | List all users | Admin |
| PUT | `/api/users/{id}` | Update user | Admin |
| DELETE | `/api/users/{id}` | Deactivate user | Admin |
| GET | `/api/logs?lines=100` | View recent logs | Admin |

---

## Running Tests

```bash
dotnet test

# Run specific project
dotnet test tests\MyDotNetApp.Application.Tests

# With detailed output
dotnet test --logger "console;verbosity=detailed"
```

---

## Adding a New Entity

Follow this 10-step guide to add a new entity (e.g., `Category`):

1. **Domain** — Create `src/MyDotNetApp.Domain/Entities/Category.cs` extending `BaseEntity`
2. **Domain** — Add `ICategoryRepository` interface extending `IRepository<Category>`
3. **Domain** — Add `ICategoryRepository Categories { get; }` to `IUnitOfWork`
4. **Application** — Add `CategoryDto`, `CreateCategoryDto`, `UpdateCategoryDto` in `DTOs/`
5. **Application** — Add `ICategoryService` interface in `Interfaces/`
6. **Application** — Implement `CategoryService` in `Services/` (mirror `ProductService`)
7. **Application** — Add mappings in `MappingProfile.cs` and register service in `DependencyInjection.cs`
8. **Infrastructure** — Implement `CategoryRepository` and update `UnitOfWork`
9. **Infrastructure** — Add `DbSet<Category>` to `AppDbContext`; run `dotnet ef migrations add AddCategory`
10. **Web** — Add `CategoryEndpoints.cs`, `Pages/Categories/Index.razor`, and nav link in `NavMenu.razor`

---

## Renaming for Your Project

1. Find & replace `MyDotNetApp` → your project name (e.g., `InventoryApp`) across all files
2. Rename solution and project folders to match
3. Update `appsettings.json`: `JwtSettings.Issuer`, `JwtSettings.Audience`, connection string
4. Change default admin credentials in `DataSeeder.cs`
5. Remove or repurpose the `Product` entity as your first real entity
6. Run `dotnet ef migrations add InitialCreate` for a fresh migration baseline

---

## Configuration

Key settings in `src/MyDotNetApp.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  },
  "JwtSettings": {
    "SecretKey": "CHANGE_THIS_SECRET_KEY_IN_PRODUCTION_MIN_32_CHARS!!",
    "ExpirationMinutes": 60
  }
}
```

> ⚠️ Always change `SecretKey` before deploying to production!

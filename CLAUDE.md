# MyDotNetApp — Complete Boilerplate Reference

Reusable ASP.NET Core 10 boilerplate using Clean Architecture.
To start a new project: find-and-replace `MyDotNetApp` with your project name across all files and folders.

---

## Environment

- **Shell:** Always use `powershell.exe -Command "..."` — CMD fails silently in this WSL setup
- **SDK Required:** .NET 10 (`net10.0`). Currently only 7.0.401 is installed → install from https://dotnet.microsoft.com/download/dotnet/10.0
- **EF CLI:** `dotnet tool install -g dotnet-ef`

---

## Technology Stack

| Concern | Technology | Notes |
|---|---|---|
| Framework | ASP.NET Core 10 | `net10.0` |
| UI | Blazor Web App | Interactive Server mode |
| UI Components | MudBlazor 7.x | Material Design |
| Database | SQLite + EF Core 10 | File: `app.db` in Web project |
| Auth | ASP.NET Identity + JWT | JWT for APIs, cascading state for Blazor |
| Logging | Serilog | Console + rolling daily file `logs/log-*.txt` |
| Real-Time | SignalR | Hub at `/hubs/notifications` |
| Testing | xUnit + Moq + FluentAssertions | |
| Mapping | AutoMapper 13 | Profiles per layer |

---

## Complete File Tree

```
MyDotNetApp/
├── MyDotNetApp.sln
├── CLAUDE.md
├── README.md
├── src/
│   ├── MyDotNetApp.Domain/
│   │   ├── MyDotNetApp.Domain.csproj
│   │   ├── Common/
│   │   │   ├── Result.cs               ← Result<T> railway pattern
│   │   │   └── PagedResult.cs          ← Pagination wrapper
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs           ← Id, CreatedAt, UpdatedAt, IsDeleted
│   │   │   └── Product.cs              ← Sample entity
│   │   └── Interfaces/
│   │       ├── IRepository.cs          ← Generic CRUD interface
│   │       ├── IProductRepository.cs   ← + SearchAsync
│   │       └── IUnitOfWork.cs          ← Products + SaveChangesAsync
│   │
│   ├── MyDotNetApp.Application/
│   │   ├── MyDotNetApp.Application.csproj
│   │   ├── DependencyInjection.cs      ← AddApplicationServices()
│   │   ├── DTOs/
│   │   │   ├── ProductDto.cs           ← ProductDto, CreateProductDto, UpdateProductDto
│   │   │   ├── Auth/
│   │   │   │   └── AuthDtos.cs         ← LoginRequestDto, LoginResponseDto, RegisterRequestDto
│   │   │   └── Users/
│   │   │       └── UserDtos.cs         ← UserDto, UpdateUserDto
│   │   ├── Interfaces/
│   │   │   ├── IProductService.cs
│   │   │   ├── IAuthService.cs
│   │   │   ├── IUserService.cs
│   │   │   └── INotificationService.cs ← BroadcastAsync, SendToUserAsync
│   │   ├── Mappings/
│   │   │   └── MappingProfile.cs       ← Product ↔ ProductDto mappings
│   │   └── Services/
│   │       └── ProductService.cs       ← Full CRUD + calls INotificationService
│   │
│   ├── MyDotNetApp.Infrastructure/
│   │   ├── MyDotNetApp.Infrastructure.csproj
│   │   ├── DependencyInjection.cs      ← AddInfrastructureServices()
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs         ← IdentityDbContext<ApplicationUser>, audit override
│   │   │   └── Seed/
│   │   │       └── DataSeeder.cs       ← Roles, admin user, 5 sample products
│   │   ├── Identity/
│   │   │   ├── ApplicationUser.cs      ← IdentityUser + IsActive + CreatedAt
│   │   │   ├── JwtTokenService.cs      ← Generates JWT from user + roles
│   │   │   ├── AuthService.cs          ← IAuthService impl (Login, Register)
│   │   │   └── UserService.cs          ← IUserService impl (CRUD on Identity users)
│   │   ├── Hubs/
│   │   │   ├── NotificationHub.cs      ← SignalR hub
│   │   │   └── SignalRNotificationService.cs ← INotificationService impl via IHubContext
│   │   └── Repositories/
│   │       ├── Repository.cs           ← Generic Repository<T> (soft-delete aware)
│   │       ├── ProductRepository.cs    ← + SearchAsync with EF LINQ
│   │       └── UnitOfWork.cs           ← IUnitOfWork impl
│   │
│   └── MyDotNetApp.Web/
│       ├── MyDotNetApp.Web.csproj
│       ├── Program.cs                  ← Full app bootstrap
│       ├── GlobalExceptionHandler.cs   ← IExceptionHandler → ProblemDetails
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Auth/
│       │   ├── JwtAuthStateProvider.cs ← AuthenticationStateProvider from localStorage JWT
│       │   └── TokenStorageService.cs  ← JS interop localStorage read/write
│       ├── Components/
│       │   ├── App.razor               ← HTML shell, MudBlazor CSS/JS
│       │   ├── Routes.razor            ← Router with AuthorizeRouteView
│       │   ├── RedirectToLogin.razor   ← Redirects unauthenticated users
│       │   ├── _Imports.razor          ← Global @using directives
│       │   ├── Layout/
│       │   │   ├── MainLayout.razor    ← MudLayout, AppBar, Drawer, logout
│       │   │   ├── NavMenu.razor       ← Nav links (Admin section behind AuthorizeView)
│       │   │   └── NotificationListener.razor ← Connects SignalR, shows toasts
│       │   └── Pages/
│       │       ├── Home.razor          ← Dashboard cards
│       │       ├── Auth/
│       │       │   ├── Login.razor     ← Calls /api/auth/login, stores JWT
│       │       │   └── Register.razor  ← Calls /api/auth/register
│       │       ├── Admin/
│       │       │   ├── Users.razor     ← User table, toggle active/inactive
│       │       │   └── Logs.razor      ← Dark-themed log viewer
│       │       └── Products/
│       │           ├── Index.razor     ← Search, paginated table, CRUD actions
│       │           └── ProductFormDialog.razor ← MudDialog for create/edit
│       ├── Endpoints/
│       │   ├── AuthEndpoints.cs        ← /api/auth/login, /api/auth/register
│       │   ├── ProductEndpoints.cs     ← /api/products (full CRUD)
│       │   ├── UserEndpoints.cs        ← /api/users (Admin only)
│       │   └── LogEndpoints.cs         ← /api/logs (Admin only, reads log file)
│       └── wwwroot/
│           └── css/
│               └── app.css
│
└── tests/
    ├── MyDotNetApp.Application.Tests/
    │   ├── MyDotNetApp.Application.Tests.csproj
    │   └── Services/
    │       └── ProductServiceTests.cs  ← 8 unit tests (GetById, Create, Update, Delete, Search)
    └── MyDotNetApp.Infrastructure.Tests/
        ├── MyDotNetApp.Infrastructure.Tests.csproj
        └── Repositories/
            └── ProductRepositoryTests.cs ← 5 integration tests (SQLite in-memory)
```

---

## Architecture — Layer Rules

```
Domain ← Application ← Infrastructure ← Web
```

| Layer | Can reference | Cannot reference |
|---|---|---|
| Domain | Nothing | Everything |
| Application | Domain only | Infrastructure, Web |
| Infrastructure | Domain + Application | Web |
| Web | Application + Infrastructure | — |

**Key rule:** Business logic and interfaces live in Domain/Application. Infrastructure only implements. Web only wires and presents.

---

## Domain Layer

### BaseEntity
All entities extend this:
```csharp
public Guid Id { get; set; }          // auto-generated
public DateTime CreatedAt { get; set; } // set on new()
public DateTime UpdatedAt { get; set; } // updated by AppDbContext.SaveChangesAsync
public bool IsDeleted { get; set; }    // soft delete flag
```

### Result Pattern
All service methods return `Result` or `Result<T>` — never throw for business errors:
```csharp
return Result.Success(dto);           // success with value
return Result.Failure("Not found.");  // business error
result.IsSuccess                      // check before accessing Value
result.Value                          // throws if IsFailure
result.Error                          // null if IsSuccess
```

### PagedResult<T>
```csharp
Items        // IEnumerable<T>
TotalCount   // total matching records
Page         // current page (1-based)
PageSize     // items per page
TotalPages   // computed
HasPreviousPage / HasNextPage
```

### Product Entity
```csharp
string Name, Description, SKU
decimal Price
int Stock
// + BaseEntity fields
```

---

## Application Layer

### Interfaces defined here (implemented in Infrastructure)
- `IAuthService` — `LoginAsync(LoginRequestDto)`, `RegisterAsync(RegisterRequestDto)`
- `IUserService` — `GetAllUsersAsync`, `GetUserByIdAsync`, `UpdateUserAsync`, `DeleteUserAsync`
- `INotificationService` — `BroadcastAsync(message)`, `SendToUserAsync(userId, message)`

### Services implemented here
- `ProductService` — implements `IProductService`, uses `IUnitOfWork` + `IMapper` + `INotificationService`

### DTOs
| DTO | Fields |
|---|---|
| `ProductDto` | Id, Name, Description, Price, SKU, Stock, CreatedAt, UpdatedAt |
| `CreateProductDto` | Name, Description, Price, SKU, Stock |
| `UpdateProductDto` | Name, Description, Price, SKU, Stock |
| `LoginRequestDto` | Email, Password |
| `LoginResponseDto` | Token, Email, UserName, Roles, ExpiresAt |
| `RegisterRequestDto` | UserName, Email, Password, ConfirmPassword |
| `UserDto` | Id, UserName, Email, IsActive, Roles, CreatedAt |
| `UpdateUserDto` | UserName, Email, IsActive, Role |

### AutoMapper Profile (Application layer)
```csharp
Product → ProductDto          (read)
CreateProductDto → Product    (create)
UpdateProductDto → Product    (update, ignores Id/CreatedAt/IsDeleted)
```
Identity user mappings are handled in **Infrastructure** to avoid cross-layer references.

### DI Registration
`builder.Services.AddApplicationServices()` registers:
- AutoMapper (scans `MappingProfile` assembly)
- `IProductService` → `ProductService` (scoped)

---

## Infrastructure Layer

### AppDbContext
- Extends `IdentityDbContext<ApplicationUser>`
- `DbSet<Product> Products`
- **Global query filter:** `Products` hides `IsDeleted == true` rows automatically
- **SaveChangesAsync override:** auto-sets `UpdatedAt` on modified entities

### ApplicationUser
Extends `IdentityUser` with:
- `bool IsActive` (default true)
- `DateTime CreatedAt`

### Repository Pattern
```
IRepository<T>          →  Repository<T>        (generic: CRUD + soft delete)
IProductRepository      →  ProductRepository    (+ SearchAsync with pagination)
IUnitOfWork             →  UnitOfWork           (Products property + SaveChangesAsync)
```
`Repository<T>.Delete()` performs **soft delete** (sets `IsDeleted = true`), not physical delete.

### JWT Token Service
Reads from `appsettings.json → JwtSettings`. Produces a signed JWT with claims:
- `sub`, `email`, `jti`, `name`, `nameidentifier` + one `role` claim per role.

### Auth/User Services (Infrastructure, implement Application interfaces)
- `AuthService` — uses `UserManager<ApplicationUser>` + `JwtTokenService`
- `UserService` — uses `UserManager<ApplicationUser>` for CRUD; `DeleteUserAsync` soft-deletes by setting `IsActive = false`

### SignalR
- `NotificationHub` — simple hub, method `BroadcastMessage(string)`
- `SignalRNotificationService` — implements `INotificationService` via `IHubContext<NotificationHub>`
- SignalR JWT: token accepted from query string `?access_token=...` for hub connections

### DataSeeder
Runs on startup via `DataSeeder.SeedAsync(app)`. Creates:
- Roles: `Admin`, `User`
- Admin user: `admin@app.com` / `Admin@123`
- 5 sample products (Laptop Pro, Wireless Mouse, Keyboard, Monitor, USB-C Hub)

### DI Registration
`builder.Services.AddInfrastructureServices(configuration)` registers:
- `AppDbContext` (SQLite)
- ASP.NET Identity with `ApplicationUser`
- JWT Bearer authentication
- SignalR
- `IUnitOfWork` → `UnitOfWork` (scoped)
- `IAuthService` → `AuthService`, `IUserService` → `UserService`
- `INotificationService` → `SignalRNotificationService`
- `JwtTokenService`

---

## Web Layer

### Program.cs — Service Registration Order
1. Serilog (bootstrap then full config)
2. `AddApplicationServices()`
3. `AddInfrastructureServices(config)`
4. `AddRazorComponents().AddInteractiveServerComponents()`
5. `AddMudServices()`
6. `TokenStorageService`, `JwtAuthStateProvider`, `AddAuthorization` (with `"AdminOnly"` policy)
7. `AddHttpClient("API", ...)` — named client with base address from `ApiBaseUrl`
8. `AddProblemDetails()` + `AddExceptionHandler<GlobalExceptionHandler>()`

### Middleware Order (important)
```
UseExceptionHandler → UseSerilogRequestLogging → UseHttpsRedirection →
UseStaticFiles → UseAuthentication → UseAuthorization → UseAntiforgery →
MapEndpoints → MapHub → MapRazorComponents
```

### Minimal API Endpoints

| Group file | Route prefix | Auth | Methods |
|---|---|---|---|
| `AuthEndpoints.cs` | `/api/auth` | Anonymous | POST /login, POST /register |
| `ProductEndpoints.cs` | `/api/products` | Required | GET /, GET /{id}, POST /, PUT /{id}, DELETE /{id} |
| `UserEndpoints.cs` | `/api/users` | AdminOnly policy | GET /, GET /{id}, PUT /{id}, DELETE /{id} |
| `LogEndpoints.cs` | `/api/logs` | AdminOnly policy | GET /?lines=100 |

Each group is an `IEndpointRouteBuilder` extension method, registered in `Program.cs` with `app.MapXxxEndpoints()`.

### Blazor Pages

| Route | File | Access |
|---|---|---|
| `/auth/login` | `Pages/Auth/Login.razor` | Anonymous |
| `/auth/register` | `Pages/Auth/Register.razor` | Anonymous |
| `/` | `Pages/Home.razor` | `[Authorize]` |
| `/products` | `Pages/Products/Index.razor` | `[Authorize]` |
| `/admin/users` | `Pages/Admin/Users.razor` | `[Authorize(Roles="Admin")]` |
| `/admin/logs` | `Pages/Admin/Logs.razor` | `[Authorize(Roles="Admin")]` |

Unauthenticated users are redirected to `/auth/login` by `RedirectToLogin.razor` inside `Routes.razor`.

### Authentication Flow (Blazor)
1. User submits login form → POST `/api/auth/login`
2. On success: JWT stored in `localStorage` via `TokenStorageService` (JS interop)
3. `JwtAuthStateProvider.NotifyUserLoginAsync(token)` parses JWT claims → notifies Blazor auth state
4. On logout: token removed from `localStorage`, auth state reset
5. `JwtAuthStateProvider.GetAuthenticationStateAsync()` — checks token expiry on every auth state read

### SignalR Real-Time Flow
1. `NotificationListener.razor` connects to `/hubs/notifications?access_token=<jwt>` after render
2. Server-side: `ProductService` calls `INotificationService.BroadcastAsync(message)` after every Create/Update/Delete
3. `SignalRNotificationService` sends to all clients via `IHubContext<NotificationHub>`
4. Client receives `"ReceiveNotification"` event → MudBlazor `ISnackbar` shows a toast

---

## Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  },
  "JwtSettings": {
    "SecretKey": "CHANGE_THIS_IN_PRODUCTION_MIN_32_CHARS!!",
    "Issuer": "MyDotNetApp",
    "Audience": "MyDotNetApp",
    "ExpirationMinutes": 60
  },
  "ApiBaseUrl": "https://localhost:5001",
  "Serilog": {
    "MinimumLevel": { "Default": "Information" }
  }
}
```

> **Production:** Always change `SecretKey`. Consider moving it to environment variables or Azure Key Vault.

---

## Testing

### Application.Tests (Unit)
File: `tests/MyDotNetApp.Application.Tests/Services/ProductServiceTests.cs`

Pattern:
```csharp
// Constructor sets up mocks
Mock<IUnitOfWork> _uowMock;
Mock<IProductRepository> _repoMock;
Mock<INotificationService> _notificationMock;
IMapper _mapper;         // real AutoMapper with MappingProfile
ProductService _sut;     // system under test

// Test naming: MethodName_Scenario_ExpectedBehavior
[Fact] GetByIdAsync_ExistingId_ReturnsProductDto()
[Fact] GetByIdAsync_NonExistentId_ReturnsFailure()
[Fact] CreateAsync_ValidDto_ReturnsCreatedProduct()
[Fact] CreateAsync_ValidDto_BroadcastsNotification()
[Fact] UpdateAsync_ExistingId_UpdatesAndReturnsDto()
[Fact] UpdateAsync_NonExistentId_ReturnsFailure()
[Fact] DeleteAsync_ExistingId_SoftDeletesProduct()
[Fact] DeleteAsync_NonExistentId_ReturnsFailure()
[Fact] SearchAsync_ReturnsPagedResult()
```

### Infrastructure.Tests (Integration)
File: `tests/MyDotNetApp.Infrastructure.Tests/Repositories/ProductRepositoryTests.cs`

Uses real `AppDbContext` with `SQLite in-memory` connection:
```csharp
var connection = new SqliteConnection("Filename=:memory:");
await connection.OpenAsync();
var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
```

Implements `IAsyncLifetime` for setup/teardown. Tests:
```
AddAsync_ThenGetById_ReturnsProduct
GetAllAsync_ReturnsAllNonDeletedProducts
SearchAsync_WithTerm_ReturnsMatchingProducts
SearchAsync_NullTerm_ReturnsAllProducts
SearchAsync_Pagination_ReturnsCorrectPage
Delete_SetsIsDeletedTrue_AndFiltersFromQueries
```

### Commands
```powershell
dotnet test
dotnet test tests\MyDotNetApp.Application.Tests
dotnet test tests\MyDotNetApp.Infrastructure.Tests
dotnet test --filter "FullyQualifiedName~ProductServiceTests"
dotnet test --logger "console;verbosity=detailed"
```

---

## Common Commands

```powershell
# Restore & build
dotnet restore
dotnet build

# Run application
dotnet run --project src\MyDotNetApp.Web

# EF Core — run from src\MyDotNetApp.Web
dotnet ef migrations add <Name>
dotnet ef database update
dotnet ef database drop          # reset database

# Tests
dotnet test

# Publish
dotnet publish src\MyDotNetApp.Web -c Release -o publish\
```

---

## Adding a New Entity (Step-by-Step)

Example entity: `Category`

### 1 — Domain: Entity
`src/MyDotNetApp.Domain/Entities/Category.cs`
```csharp
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
```

### 2 — Domain: Repository Interface
`src/MyDotNetApp.Domain/Interfaces/ICategoryRepository.cs`
```csharp
public interface ICategoryRepository : IRepository<Category>
{
    Task<PagedResult<Category>> SearchAsync(string? term, int page, int pageSize, CancellationToken ct = default);
}
```

### 3 — Domain: Add to IUnitOfWork
```csharp
ICategoryRepository Categories { get; }
```

### 4 — Application: DTOs
`src/MyDotNetApp.Application/DTOs/CategoryDto.cs`
— `CategoryDto`, `CreateCategoryDto`, `UpdateCategoryDto`

### 5 — Application: Service Interface
`src/MyDotNetApp.Application/Interfaces/ICategoryService.cs`
— mirror `IProductService`

### 6 — Application: Service Implementation
`src/MyDotNetApp.Application/Services/CategoryService.cs`
— mirror `ProductService`

### 7 — Application: AutoMapper + DI
Add to `MappingProfile.cs`:
```csharp
CreateMap<Category, CategoryDto>();
CreateMap<CreateCategoryDto, Category>();
CreateMap<UpdateCategoryDto, Category>().ForMember(d => d.Id, o => o.Ignore())...;
```
Add to `DependencyInjection.cs`:
```csharp
services.AddScoped<ICategoryService, CategoryService>();
```

### 8 — Infrastructure: Repository
`src/MyDotNetApp.Infrastructure/Repositories/CategoryRepository.cs` — mirror `ProductRepository.cs`

Update `UnitOfWork.cs`:
```csharp
public ICategoryRepository Categories { get; }
// initialize in constructor: Categories = new CategoryRepository(context);
```

### 9 — Infrastructure: DbContext
`AppDbContext.cs`:
```csharp
public DbSet<Category> Categories => Set<Category>();
// add HasQueryFilter in OnModelCreating
```

Run migration:
```powershell
cd src\MyDotNetApp.Web
dotnet ef migrations add AddCategory
dotnet ef database update
```

### 10 — Web: API Endpoints
`src/MyDotNetApp.Web/Endpoints/CategoryEndpoints.cs` — mirror `ProductEndpoints.cs`
Register in `Program.cs`: `app.MapCategoryEndpoints();`

### 11 — Web: Blazor Pages
`src/MyDotNetApp.Web/Components/Pages/Categories/Index.razor` — mirror `Products/Index.razor`
`src/MyDotNetApp.Web/Components/Pages/Categories/CategoryFormDialog.razor` — mirror `ProductFormDialog.razor`

Add nav link in `NavMenu.razor`:
```razor
<MudNavLink Href="/categories" Icon="@Icons.Material.Filled.Category">Categories</MudNavLink>
```

---

## Renaming the Boilerplate

```powershell
# Find and replace in all files
Get-ChildItem -Recurse -File | ForEach-Object {
    (Get-Content $_.FullName -Raw) -replace 'MyDotNetApp', 'YourProjectName' |
    Set-Content $_.FullName
}
# Then rename folders manually:
# src\MyDotNetApp.Domain → src\YourProjectName.Domain  (etc.)
```

After renaming:
1. Update `appsettings.json` — change `Issuer`, `Audience`, `SecretKey`
2. Change admin credentials in `DataSeeder.cs`
3. Delete `Product` entity or repurpose it
4. Run `dotnet ef migrations add InitialCreate` for fresh migration

---

## Password Requirements (Identity)
- Minimum 8 characters
- Requires: digit, lowercase, uppercase, non-alphanumeric
- Unique email enforced

## Roles
- `Admin` — full access including /admin/users, /admin/logs, /api/users, /api/logs
- `User` — access to authenticated pages and product endpoints

## Default Credentials (seeded)
- **Admin:** `admin@app.com` / `Admin@123`

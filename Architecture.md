# Clean Architecture — How It Works in This Boilerplate

This document explains what Clean Architecture is, why it is used, and exactly what each folder and file in `src/` does.

---

## The Core Idea

Clean Architecture organises code into concentric layers. The inner layers contain business rules; the outer layers contain implementation details. The rule is simple:

> **Dependencies only point inward. Inner layers never know about outer layers.**

```
┌──────────────────────────────────┐
│             Web                  │  ← UI, API endpoints, DI wiring
│  ┌────────────────────────────┐  │
│  │       Infrastructure       │  │  ← Database, JWT, SignalR, Email
│  │  ┌──────────────────────┐  │  │
│  │  │     Application      │  │  │  ← Business logic, use cases, DTOs
│  │  │  ┌────────────────┐  │  │  │
│  │  │  │     Domain     │  │  │  │  ← Entities, interfaces, rules
│  │  │  └────────────────┘  │  │  │
│  │  └──────────────────────┘  │  │
│  └────────────────────────────┘  │
└──────────────────────────────────┘
```

| Layer | Project | Can reference | Cannot reference |
|---|---|---|---|
| Domain | `MyDotNetApp.Domain` | Nothing | Everything |
| Application | `MyDotNetApp.Application` | Domain | Infrastructure, Web |
| Infrastructure | `MyDotNetApp.Infrastructure` | Domain + Application | Web |
| Web | `MyDotNetApp.Web` | Application + Infrastructure | — |

---

## Layer 1 — Domain (`src/MyDotNetApp.Domain/`)

**What it is:** The heart of the application. Contains pure business concepts with zero dependencies on any framework, database, or library.

**Rule:** No NuGet packages. No EF Core. No ASP.NET. Just C# classes and interfaces.

### Folders

```
MyDotNetApp.Domain/
├── Common/
│   ├── Result.cs           ← Railway-oriented error handling
│   └── PagedResult.cs      ← Pagination wrapper for list results
├── Entities/
│   ├── BaseEntity.cs       ← Base class all entities inherit
│   └── Product.cs          ← Sample business entity
└── Interfaces/
    ├── IRepository.cs      ← Generic data access contract
    ├── IProductRepository.cs ← Product-specific data contract
    └── IUnitOfWork.cs      ← Transaction boundary contract
```

### Key concepts

**`BaseEntity`** — Every database entity inherits from this:
```csharp
public Guid Id { get; set; }         // primary key, auto-generated
public DateTime CreatedAt { get; set; }
public DateTime UpdatedAt { get; set; }
public bool IsDeleted { get; set; }  // soft delete — record is hidden, not removed
```

**`Result<T>`** — Services return this instead of throwing exceptions for business errors:
```csharp
// In a service method:
if (product is null)
    return Result.Failure<ProductDto>("Product not found.");

return Result.Success(dto);

// Caller checks:
if (result.IsSuccess)
    return Results.Ok(result.Value);
else
    return Results.NotFound(result.Error);
```

**`IRepository<T>` / `IUnitOfWork`** — The Domain defines *what* data operations are needed. Infrastructure provides *how* they work. Domain never imports EF Core.

---

## Layer 2 — Application (`src/MyDotNetApp.Application/`)

**What it is:** Orchestrates business use cases. Calls repositories (via interfaces), maps data between layers, and defines what services the app needs.

**Rule:** Depends on Domain only. Never imports EF Core, Identity, or any infrastructure library.

### Folders

```
MyDotNetApp.Application/
├── DependencyInjection.cs  ← Registers Application services into DI container
├── DTOs/
│   ├── ProductDto.cs       ← Data shapes for Products (read, create, update)
│   ├── Auth/
│   │   └── AuthDtos.cs     ← LoginRequestDto, LoginResponseDto, RegisterRequestDto
│   └── Users/
│       └── UserDtos.cs     ← UserDto, UpdateUserDto
├── Interfaces/
│   ├── IProductService.cs  ← Contract: what the product use cases can do
│   ├── IAuthService.cs     ← Contract: login and register
│   ├── IUserService.cs     ← Contract: user management
│   └── INotificationService.cs ← Contract: real-time broadcast (SignalR lives in Infrastructure)
├── Mappings/
│   └── MappingProfile.cs   ← AutoMapper: Product ↔ ProductDto, etc.
└── Services/
    └── ProductService.cs   ← Implements IProductService using IUnitOfWork + IMapper
```

### Key concepts

**DTOs (Data Transfer Objects)** — Application never exposes raw entities to the outside world. Entities are internal; DTOs are what callers send and receive:

| Scenario | DTO Used |
|---|---|
| Reading a product | `ProductDto` |
| Creating a product | `CreateProductDto` |
| Updating a product | `UpdateProductDto` |
| Logging in | `LoginRequestDto` → `LoginResponseDto` |

**Services** — Application services contain the use case logic:
```
ProductService.CreateAsync(dto)
  1. Map CreateProductDto → Product entity
  2. Call _uow.Products.AddAsync(product)
  3. Call _uow.SaveChangesAsync()
  4. Broadcast SignalR notification via INotificationService
  5. Return Result.Success(ProductDto)
```

**Interfaces** — `INotificationService` is *defined* here in Application but *implemented* in Infrastructure. Application knows it can broadcast a message; it does not know or care that SignalR is used.

---

## Layer 3 — Infrastructure (`src/MyDotNetApp.Infrastructure/`)

**What it is:** All technical details — database, authentication, external services. Implements the contracts defined in Domain and Application.

**Rule:** Depends on Domain + Application. Never imported directly by the domain or application logic.

### Folders

```
MyDotNetApp.Infrastructure/
├── DependencyInjection.cs   ← Registers all Infrastructure services into DI
├── Data/
│   ├── AppDbContext.cs      ← EF Core DbContext (SQLite), global soft-delete filter
│   └── Seed/
│       └── DataSeeder.cs    ← Runs on startup: creates roles, admin user, sample products
├── Identity/
│   ├── ApplicationUser.cs   ← Extends IdentityUser with IsActive + CreatedAt
│   ├── JwtTokenService.cs   ← Generates signed JWT tokens from user + roles
│   ├── AuthService.cs       ← Implements IAuthService (login, register)
│   └── UserService.cs       ← Implements IUserService (list, update, deactivate)
├── Hubs/
│   ├── NotificationHub.cs          ← SignalR hub class
│   └── SignalRNotificationService.cs ← Implements INotificationService via IHubContext
└── Repositories/
    ├── Repository.cs        ← Generic EF Core repository (CRUD + soft delete)
    ├── ProductRepository.cs ← Extends Repository<Product> with SearchAsync
    └── UnitOfWork.cs        ← Wraps DbContext, exposes repositories + SaveChangesAsync
```

### Key concepts

**`AppDbContext`** — The EF Core database context:
- Inherits from `IdentityDbContext<ApplicationUser>` (gets all Identity tables for free)
- Has a global query filter on `Products` so soft-deleted records are never returned unless explicitly requested
- Overrides `SaveChangesAsync` to auto-set `UpdatedAt` on every modified entity

**Repository pattern** — Abstracts database queries behind interfaces:
```
IRepository<T>       →  Repository<T>       (Add, GetById, GetAll, Update, Delete)
IProductRepository   →  ProductRepository   (+ SearchAsync with EF LINQ)
IUnitOfWork          →  UnitOfWork          (Products property + SaveChangesAsync)
```
`Delete()` performs a *soft delete* — sets `IsDeleted = true` rather than removing the row.

**JWT** — `JwtTokenService` reads `JwtSettings` from `appsettings.json` and produces a signed token containing `sub`, `email`, `name`, `role` claims.

**SignalR** — `SignalRNotificationService` receives calls from `ProductService` (via `INotificationService`) and broadcasts to all connected clients using `IHubContext<NotificationHub>`.

---

## Layer 4 — Web (`src/MyDotNetApp.Web/`)

**What it is:** The entry point. Wires everything together, hosts the Blazor UI, and exposes Minimal API endpoints.

**Rule:** Can reference Application and Infrastructure. Contains no business logic — only presentation, routing, and DI registration.

### Folders

```
MyDotNetApp.Web/
├── Program.cs               ← App bootstrap: registers all services, configures middleware
├── GlobalExceptionHandler.cs ← Catches unhandled exceptions, returns ProblemDetails JSON
├── appsettings.json         ← JWT settings, connection string, Serilog config, port
├── Auth/
│   ├── JwtAuthStateProvider.cs ← Reads JWT from localStorage, provides Blazor auth state
│   ├── TokenStorageService.cs  ← JS interop: read/write/remove token in localStorage
│   └── ApiClient.cs            ← Creates authenticated HttpClient (attaches Bearer token)
├── Components/
│   ├── App.razor            ← HTML shell (head, body, Blazor script tags)
│   ├── Routes.razor         ← Blazor router + AuthorizeRouteView + error pages
│   ├── RedirectToLogin.razor ← Navigates to /auth/login?returnUrl=... when not authenticated
│   ├── _Imports.razor       ← Global @using statements for all Razor files
│   ├── Layout/
│   │   ├── MainLayout.razor        ← AppBar, drawer, logout button
│   │   ├── NavMenu.razor           ← Sidebar navigation links
│   │   └── NotificationListener.razor ← Connects to SignalR hub, shows toast notifications
│   └── Pages/
│       ├── Home.razor              ← Dashboard (requires auth)
│       ├── Auth/
│       │   ├── Login.razor         ← Login form → POST /api/auth/login → store JWT
│       │   └── Register.razor      ← Register form → POST /api/auth/register
│       ├── Admin/
│       │   ├── Users.razor         ← User table, toggle active/inactive (Admin only)
│       │   └── Logs.razor          ← Log viewer with colour coding (Admin only)
│       └── Products/
│           ├── Index.razor         ← Product list, search, CRUD (authenticated)
│           └── ProductFormDialog.razor ← MudDialog for create/edit product
├── Endpoints/
│   ├── AuthEndpoints.cs     ← POST /api/auth/login, POST /api/auth/register
│   ├── ProductEndpoints.cs  ← GET/POST/PUT/DELETE /api/products
│   ├── UserEndpoints.cs     ← GET/PUT/DELETE /api/users (Admin only)
│   └── LogEndpoints.cs      ← GET /api/logs (Admin only)
└── wwwroot/
    └── css/
        └── app.css          ← Global styles
```

### Key concepts

**`Program.cs`** — Registers all layers' services and configures the middleware pipeline:
```
HTTP Request
    → UseExceptionHandler
    → UseSerilogRequestLogging
    → UseHttpsRedirection
    → UseStaticFiles
    → UseAuthentication      ← validates JWT Bearer token
    → UseAuthorization       ← enforces [Authorize] on API endpoints
    → UseAntiforgery
    → MapAuthEndpoints / MapProductEndpoints / MapUserEndpoints / MapLogEndpoints
    → MapHub (SignalR)
    → MapRazorComponents     ← serves the Blazor app (AllowAnonymous — auth handled client-side)
```

**Auth in Blazor** — JWT lives in `localStorage` (not cookies). Because JS interop is required to read it, prerendering is disabled (`InteractiveServerRenderMode(prerender: false)`). The Blazor circuit starts, `JwtAuthStateProvider` reads the token, and `AuthorizeRouteView` redirects unauthenticated users to `/auth/login`.

**Minimal API Endpoints** — Each file in `Endpoints/` is a static class with an extension method on `IEndpointRouteBuilder`. This keeps endpoint definitions out of `Program.cs`:
```csharp
// ProductEndpoints.cs
public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/products").RequireAuthorization();
    group.MapGet("/", async (IProductService svc, ...) => ...);
    // etc.
    return app;
}
```

---

## How a Request Flows Through the Layers

Example: **User creates a product via the Blazor UI**

```
1. USER clicks "Save" in ProductFormDialog.razor (Web/Components)
        ↓
2. ProductFormDialog calls POST /api/products via ApiClient.CreateAsync()
   (ApiClient attaches Bearer JWT to the request)
        ↓
3. ProductEndpoints.cs receives the request, calls IProductService.CreateAsync(dto)
   (Web → Application interface)
        ↓
4. ProductService.CreateAsync() in Application:
   a. Maps CreateProductDto → Product entity (AutoMapper)
   b. Calls _uow.Products.AddAsync(product)      (Application → Domain interface)
   c. Calls _uow.SaveChangesAsync()
   d. Calls _notifications.BroadcastAsync(...)   (Application → Application interface)
        ↓
5. UnitOfWork.SaveChangesAsync() calls AppDbContext.SaveChangesAsync()
   (Infrastructure implements the Domain interface)
        ↓
6. SignalRNotificationService.BroadcastAsync() sends via IHubContext<NotificationHub>
   (Infrastructure implements the Application interface)
        ↓
7. All connected browser clients receive "ReceiveNotification" → toast appears
   (Web/Components/Layout/NotificationListener.razor)
        ↓
8. ProductService returns Result.Success(ProductDto) → endpoint returns 201 Created
        ↓
9. ProductFormDialog closes, Index.razor calls LoadProducts() → table refreshes
```

---

## Why This Structure?

| Benefit | How it's achieved |
|---|---|
| **Testable** | Application services depend on interfaces → mock them in unit tests |
| **Swappable** | Replace SQLite with PostgreSQL by changing only Infrastructure |
| **Readable** | Each file has one job; you know exactly where to look |
| **No circular deps** | Domain has no imports; each layer only imports layers inside it |
| **Framework-independent** | Domain and Application have no ASP.NET or EF Core imports |

# Getting Started with MyDotNetApp Boilerplate

A step-by-step guide to get the boilerplate running from scratch.

---

## Prerequisites

Install all of the following before proceeding.

### 1. .NET 10 SDK
Required version: **10.0 or later**

Download: https://dotnet.microsoft.com/download/dotnet/10.0

Verify:
```powershell
dotnet --version
# Expected output: 10.x.x
```

### 2. EF Core CLI Tool
```powershell
# Install (first time)
dotnet tool install -g dotnet-ef

# Already installed but on an older version? Update it:
dotnet tool update -g dotnet-ef
```

Verify:
```powershell
dotnet ef --version
# Expected output: Entity Framework Core .NET Command-line Tools 10.x.x
```

> If you get "command not found", add the dotnet tools path to your system PATH:
> `%USERPROFILE%\.dotnet\tools`
>
> **Must be v10.x.** Running an older version (e.g. 7.x or 8.x) against a .NET 10 project will fail.

### 3. Git
Download: https://git-scm.com

### 4. IDE (choose one)
- **Visual Studio 2022** (v17.8+) — https://visualstudio.microsoft.com
- **VS Code** — https://code.visualstudio.com
  - Install extension: **C# Dev Kit**

### 5. SQLite Viewer (optional but useful)
DB Browser for SQLite — https://sqlitebrowser.org
Lets you browse `app.db` to inspect seeded data.

---

## First-Time Setup

### Step 1 — Get the project

If cloning from GitHub:
```powershell
git clone https://github.com/juanyean/dotnetboilerplate.git
cd dotnetboilerplate
```


### Step 2 — Restore NuGet packages
```powershell
dotnet restore
```

Expected: all packages restored, no errors.

### Step 3 — Create the initial database migration

> This only needs to be done **once** (or after you add a new entity).

Run from the **solution root**:
```cmd
dotnet ef migrations add InitialCreate --project src\MyDotNetApp.Infrastructure --startup-project src\MyDotNetApp.Web
```

- `--project` — where the migration files are written (`Infrastructure`)
- `--startup-project` — where `appsettings.json` and the connection string live (`Web`)

Expected: a `Migrations/` folder appears inside `src\MyDotNetApp.Infrastructure\`.

### Step 4 — Run the application
```powershell
# From the solution root
dotnet run --project src\MyDotNetApp.Web
```

On first run the app will automatically:
- Create `app.db` (SQLite database file)
- Apply the migration (create all tables)
- Seed: Admin & User roles, admin account, 5 sample products

Expected output in terminal:
```
[INF] Now listening on: https://localhost:5051
[INF] Application started.
```

### Step 5 — Open the app
Navigate to: **https://localhost:5051**

> Your browser may warn about the development SSL certificate.
> To trust it once: `dotnet dev-certs https --trust`

Unauthenticated users are **automatically redirected** to the login page. You will not see a 401 error — the app handles this gracefully and returns you to your original URL after login.

---

## Verifying All Features

Use the checklist below to confirm everything is working.

### Authentication

| Action | URL | Credentials |
|---|---|---|
| Login as Admin | https://localhost:5051/auth/login | `admin@app.com` / `Admin@123` |
| Register new user | https://localhost:5051/auth/register | any email + strong password |
| Logout | Click Logout in the top bar | — |

### Products (CRUD)

Navigate to: **https://localhost:5051/products**

| Test | Expected result |
|---|---|
| Page loads | Table shows 5 seeded products |
| Search "laptop" | Filters to 1 result |
| Click **New Product** | Dialog opens |
| Fill form and save | New product appears in table |
| Click edit (pencil icon) | Dialog pre-fills with product data |
| Click delete (trash icon) | Confirmation prompt, product removed |

### SignalR — Real-Time Notifications

1. Open two browser windows/tabs, both logged in
2. In window **A**, create or delete a product
3. In window **B**, a **toast notification** should appear automatically

### Admin — User Management

Login as Admin and navigate to: **https://localhost:5051/admin/users**

| Test | Expected result |
|---|---|
| Page loads | Table lists all registered users |
| Click edit on a user | Active/Inactive toggles |
| Login as regular User | Styled "Access Denied" page with lock icon and "Go to Home" button |

### Admin — Logs

Navigate to: **https://localhost:5051/admin/logs**

| Test | Expected result |
|---|---|
| Page loads | Recent log entries displayed |
| Logs are colour-coded | Errors in red, warnings in yellow |
| Refresh button | Loads latest entries |

---

## Running the Tests

```powershell
# Run all tests from the solution root
dotnet test

# Expected output
# Passed! - Failed: 0, Passed: 13, Skipped: 0
```

### Run a specific test project
```powershell
dotnet test tests\MyDotNetApp.Application.Tests
dotnet test tests\MyDotNetApp.Infrastructure.Tests
```

### Run a specific test class
```powershell
dotnet test --filter "FullyQualifiedName~ProductServiceTests"
dotnet test --filter "FullyQualifiedName~ProductRepositoryTests"
```

### Verbose output
```powershell
dotnet test --logger "console;verbosity=detailed"
```

---

## Common Issues & Fixes

### "dotnet" is not recognised
.NET SDK not installed or not on PATH. Re-install from https://dotnet.microsoft.com/download/dotnet/10.0

### "dotnet ef" is not recognised
```powershell
dotnet tool install -g dotnet-ef
# Also ensure tools are on PATH:
# Add %USERPROFILE%\.dotnet\tools to system PATH
```

### SSL certificate error in browser
```powershell
dotnet dev-certs https --trust
# Restart browser after running
```

### Migration errors on startup
```cmd
dotnet ef database drop --force --project src\MyDotNetApp.Infrastructure --startup-project src\MyDotNetApp.Web
dotnet ef database update --project src\MyDotNetApp.Infrastructure --startup-project src\MyDotNetApp.Web
```

### Port already in use
Edit `src\MyDotNetApp.Web\Properties\launchSettings.json` and change the port numbers, or kill the process using the port:
```powershell
# Find what's on port 5051
netstat -ano | findstr :5051
# Kill by PID
taskkill /PID <PID> /F
```

### Redirected to login on every page load (even when logged in)
This happens if prerendering is accidentally re-enabled. Confirm `App.razor` uses:
```razor
<Routes @rendermode="new InteractiveServerRenderMode(prerender: false)" />
```
Prerendering must be disabled because auth reads from `localStorage` via JS interop, which is unavailable during server-side prerender.

### Build errors after adding a new entity
Make sure you:
1. Added `DbSet<T>` to `AppDbContext.cs`
2. Created and ran a new migration from the solution root:
   ```powershell
   dotnet ef migrations add <Name> --project src\MyDotNetApp.Infrastructure --startup-project src\MyDotNetApp.Web
   dotnet ef database update  --project src\MyDotNetApp.Infrastructure --startup-project src\MyDotNetApp.Web
   ```
3. Registered the service in the correct `DependencyInjection.cs`

---

## What's Next?

- **Add a new entity** — follow the 11-step guide in `CLAUDE.md` using `Category` as the example
- **Adapt for your project** — rename `MyDotNetApp` to your project name (see Renaming section in `CLAUDE.md`)
- **Push to GitHub** — see `README.md` for git commands
- **Change the JWT secret** — update `JwtSettings.SecretKey` in `appsettings.json` before any real deployment

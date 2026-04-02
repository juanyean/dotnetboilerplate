using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using MyDotNetApp.Application;
using MyDotNetApp.Infrastructure;
using MyDotNetApp.Infrastructure.Data.Seed;
using MyDotNetApp.Infrastructure.Hubs;
using MyDotNetApp.Web;
using MyDotNetApp.Web.Auth;
using MyDotNetApp.Web.Endpoints;
using Serilog;
using Serilog.Events;

// ── Serilog bootstrap logger ────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog full configuration ───────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

    // ── Application & Infrastructure services ───────────────────────────────
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // ── Blazor ───────────────────────────────────────────────────────────────
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddMudServices();

    // ── Auth state for Blazor ────────────────────────────────────────────────
    builder.Services.AddScoped<TokenStorageService>();
    builder.Services.AddScoped<JwtAuthStateProvider>();
    builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    });
    builder.Services.AddCascadingAuthenticationState();

    // ── Named HTTP client for Blazor pages calling Minimal API ──────────────
    builder.Services.AddHttpClient("API", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001");
    });

    // ── HTTP Context accessor ─────────────────��──────────────────────────────
    builder.Services.AddHttpContextAccessor();

    // ── Global exception handling ────────────────────────────────────────────
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    var app = builder.Build();

    // ── Middleware pipeline ──────────────────────────────────────────────────
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler();
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseAntiforgery();

    // ── Minimal API endpoints ────────────────────────────────────────────────
    app.MapAuthEndpoints();
    app.MapProductEndpoints();
    app.MapUserEndpoints();
    app.MapLogEndpoints();

    // ── SignalR hub ──────────────────────────────────────────────────────────
    app.MapHub<NotificationHub>("/hubs/notifications");

    // ── Blazor ───────────────────────────────────────────────────────────────
    app.MapRazorComponents<MyDotNetApp.Web.Components.App>()
        .AddInteractiveServerRenderMode();

    // ── Seed database ────────────────────────────────────────────────────────
    await DataSeeder.SeedAsync(app);

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

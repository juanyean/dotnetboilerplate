using MyDotNetApp.Application.DTOs.Auth;
using MyDotNetApp.Application.Interfaces;

namespace MyDotNetApp.Web.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequestDto dto, IAuthService authService) =>
        {
            var result = await authService.LoginAsync(dto);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Unauthorized();
        })
        .WithName("Login")
        .AllowAnonymous();

        group.MapPost("/register", async (RegisterRequestDto dto, IAuthService authService) =>
        {
            var result = await authService.RegisterAsync(dto);
            return result.IsSuccess
                ? Results.Ok(new { message = "Registration successful." })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("Register")
        .AllowAnonymous();

        return app;
    }
}

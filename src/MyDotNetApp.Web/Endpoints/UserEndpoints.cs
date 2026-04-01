using MyDotNetApp.Application.DTOs.Users;
using MyDotNetApp.Application.Interfaces;

namespace MyDotNetApp.Web.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/", async (IUserService svc) =>
        {
            var result = await svc.GetAllUsersAsync();
            return Results.Ok(result.Value);
        })
        .WithName("GetAllUsers");

        group.MapGet("/{id}", async (string id, IUserService svc) =>
        {
            var result = await svc.GetUserByIdAsync(id);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("GetUser");

        group.MapPut("/{id}", async (string id, UpdateUserDto dto, IUserService svc) =>
        {
            var result = await svc.UpdateUserAsync(id, dto);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("UpdateUser");

        group.MapDelete("/{id}", async (string id, IUserService svc) =>
        {
            var result = await svc.DeleteUserAsync(id);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("DeleteUser");

        return app;
    }
}

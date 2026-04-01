using Microsoft.AspNetCore.Authorization;
using MyDotNetApp.Application.DTOs;
using MyDotNetApp.Application.Interfaces;

namespace MyDotNetApp.Web.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .RequireAuthorization();

        group.MapGet("/", async (
            IProductService svc,
            string? search = null,
            int page = 1,
            int pageSize = 10) =>
        {
            var result = await svc.SearchAsync(search, page, pageSize);
            return Results.Ok(result.Value);
        })
        .WithName("SearchProducts");

        group.MapGet("/{id:guid}", async (Guid id, IProductService svc) =>
        {
            var result = await svc.GetByIdAsync(id);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("GetProduct");

        group.MapPost("/", async (CreateProductDto dto, IProductService svc) =>
        {
            var result = await svc.CreateAsync(dto);
            return result.IsSuccess
                ? Results.Created($"/api/products/{result.Value.Id}", result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("CreateProduct");

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductDto dto, IProductService svc) =>
        {
            var result = await svc.UpdateAsync(id, dto);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("UpdateProduct");

        group.MapDelete("/{id:guid}", async (Guid id, IProductService svc) =>
        {
            var result = await svc.DeleteAsync(id);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("DeleteProduct");

        return app;
    }
}

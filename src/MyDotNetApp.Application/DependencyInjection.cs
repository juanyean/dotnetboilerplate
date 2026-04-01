using Microsoft.Extensions.DependencyInjection;
using MyDotNetApp.Application.Interfaces;
using MyDotNetApp.Application.Mappings;
using MyDotNetApp.Application.Services;

namespace MyDotNetApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        services.AddScoped<IProductService, ProductService>();
        return services;
    }
}

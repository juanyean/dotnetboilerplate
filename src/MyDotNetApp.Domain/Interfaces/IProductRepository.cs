using MyDotNetApp.Domain.Common;
using MyDotNetApp.Domain.Entities;

namespace MyDotNetApp.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<PagedResult<Product>> SearchAsync(string? term, int page, int pageSize, CancellationToken ct = default);
}

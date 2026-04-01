using Microsoft.EntityFrameworkCore;
using MyDotNetApp.Domain.Common;
using MyDotNetApp.Domain.Entities;
using MyDotNetApp.Domain.Interfaces;
using MyDotNetApp.Infrastructure.Data;

namespace MyDotNetApp.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<PagedResult<Product>> SearchAsync(
        string? term, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(p =>
                p.Name.Contains(term) ||
                p.Description.Contains(term) ||
                p.SKU.Contains(term));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

using MyDotNetApp.Application.DTOs;
using MyDotNetApp.Domain.Common;

namespace MyDotNetApp.Application.Interfaces;

public interface IProductService
{
    Task<Result<PagedResult<ProductDto>>> SearchAsync(string? term, int page, int pageSize, CancellationToken ct = default);
    Task<Result<IEnumerable<ProductDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
    Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

using AutoMapper;
using MyDotNetApp.Application.DTOs;
using MyDotNetApp.Application.Interfaces;
using MyDotNetApp.Domain.Common;
using MyDotNetApp.Domain.Entities;
using MyDotNetApp.Domain.Interfaces;

namespace MyDotNetApp.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly INotificationService _notifications;

    public ProductService(IUnitOfWork uow, IMapper mapper, INotificationService notifications)
    {
        _uow = uow;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<Result<PagedResult<ProductDto>>> SearchAsync(
        string? term, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _uow.Products.SearchAsync(term, page, pageSize, ct);
        var result = new PagedResult<ProductDto>
        {
            Items = _mapper.Map<IEnumerable<ProductDto>>(paged.Items),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
        return Result.Success(result);
    }

    public async Task<Result<IEnumerable<ProductDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var products = await _uow.Products.GetAllAsync(ct);
        return Result.Success(_mapper.Map<IEnumerable<ProductDto>>(products));
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetByIdAsync(id, ct);
        if (product is null)
            return Result.Failure<ProductDto>("Product not found.");

        return Result.Success(_mapper.Map<ProductDto>(product));
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        var product = _mapper.Map<Product>(dto);
        await _uow.Products.AddAsync(product, ct);
        await _uow.SaveChangesAsync(ct);

        await _notifications.BroadcastAsync($"Product '{product.Name}' was created.", ct);

        return Result.Success(_mapper.Map<ProductDto>(product));
    }

    public async Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetByIdAsync(id, ct);
        if (product is null)
            return Result.Failure<ProductDto>("Product not found.");

        _mapper.Map(dto, product);
        product.UpdatedAt = DateTime.UtcNow;
        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(ct);

        await _notifications.BroadcastAsync($"Product '{product.Name}' was updated.", ct);

        return Result.Success(_mapper.Map<ProductDto>(product));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetByIdAsync(id, ct);
        if (product is null)
            return Result.Failure("Product not found.");

        _uow.Products.Delete(product);
        await _uow.SaveChangesAsync(ct);

        await _notifications.BroadcastAsync($"Product '{product.Name}' was deleted.", ct);

        return Result.Success();
    }
}

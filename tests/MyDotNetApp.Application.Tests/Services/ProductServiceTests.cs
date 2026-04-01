using AutoMapper;
using FluentAssertions;
using Moq;
using MyDotNetApp.Application.DTOs;
using MyDotNetApp.Application.Interfaces;
using MyDotNetApp.Application.Mappings;
using MyDotNetApp.Application.Services;
using MyDotNetApp.Domain.Common;
using MyDotNetApp.Domain.Entities;
using MyDotNetApp.Domain.Interfaces;

namespace MyDotNetApp.Application.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IProductRepository> _repoMock;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly IMapper _mapper;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _repoMock = new Mock<IProductRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _uowMock.Setup(u => u.Products).Returns(_repoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _notificationMock = new Mock<INotificationService>();
        _notificationMock
            .Setup(n => n.BroadcastAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new ProductService(_uowMock.Object, _mapper, _notificationMock.Object);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsProductDto()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Name = "Test Product", Price = 9.99m, SKU = "TST-001", Stock = 10 };
        _repoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        // Act
        var result = await _sut.GetByIdAsync(product.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(product.Id);
        result.Value.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsFailure()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Product not found.");
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedProduct()
    {
        // Arrange
        var dto = new CreateProductDto { Name = "New Product", Price = 19.99m, SKU = "NEW-001", Stock = 5 };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Product");
        result.Value.Price.Should().Be(19.99m);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Product>(), default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_BroadcastsNotification()
    {
        // Arrange
        var dto = new CreateProductDto { Name = "Notified Product", Price = 5m, SKU = "NOT-001", Stock = 1 };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>(), default)).Returns(Task.CompletedTask);

        // Act
        await _sut.CreateAsync(dto);

        // Assert
        _notificationMock.Verify(n => n.BroadcastAsync(
            It.Is<string>(msg => msg.Contains("Notified Product")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingId_UpdatesAndReturnsDto()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Name = "Old Name", Price = 5m, SKU = "OLD-001", Stock = 1 };
        _repoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        var updateDto = new UpdateProductDto { Name = "New Name", Price = 15m, SKU = "NEW-001", Stock = 10 };

        // Act
        var result = await _sut.UpdateAsync(product.Id, updateDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        result.Value.Price.Should().Be(15m);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentId_ReturnsFailure()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.UpdateAsync(Guid.NewGuid(), new UpdateProductDto());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Product not found.");
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingId_SoftDeletesProduct()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Name = "Delete Me", Price = 1m, SKU = "DEL-001", Stock = 0 };
        _repoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        // Act
        var result = await _sut.DeleteAsync(product.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repoMock.Verify(r => r.Delete(product), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_ReturnsFailure()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.DeleteAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Product not found.");
    }

    // ── SearchAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_ReturnsPagedResult()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Alpha", Price = 1m, SKU = "A-001", Stock = 5 },
            new() { Id = Guid.NewGuid(), Name = "Beta",  Price = 2m, SKU = "B-001", Stock = 3 }
        };
        var pagedResult = new PagedResult<Product>
        {
            Items = products,
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };
        _repoMock.Setup(r => r.SearchAsync(null, 1, 10, default)).ReturnsAsync(pagedResult);

        // Act
        var result = await _sut.SearchAsync(null, 1, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);
    }
}

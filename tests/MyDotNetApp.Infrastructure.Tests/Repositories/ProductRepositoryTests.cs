using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyDotNetApp.Domain.Entities;
using MyDotNetApp.Infrastructure.Data;
using MyDotNetApp.Infrastructure.Identity;
using MyDotNetApp.Infrastructure.Repositories;

namespace MyDotNetApp.Infrastructure.Tests.Repositories;

public class ProductRepositoryTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private AppDbContext _context = null!;
    private ProductRepository _repository = null!;

    public async Task InitializeAsync()
    {
        // Use in-memory SQLite for isolated tests
        _connection = new SqliteConnection("Filename=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _repository = new ProductRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    // ── AddAsync / GetByIdAsync ───────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsProduct()
    {
        // Arrange
        var product = new Product { Name = "Test", Price = 9.99m, SKU = "TST-001", Stock = 10 };

        // Act
        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();
        var fetched = await _repository.GetByIdAsync(product.Id);

        // Assert
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("Test");
        fetched.SKU.Should().Be("TST-001");
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllNonDeletedProducts()
    {
        // Arrange
        var active = new Product { Name = "Active", Price = 1m, SKU = "A-001", Stock = 1 };
        var deleted = new Product { Name = "Deleted", Price = 1m, SKU = "D-001", Stock = 0, IsDeleted = true };
        await _repository.AddAsync(active);
        await _context.Products.AddAsync(deleted); // bypass repo to set IsDeleted
        await _context.SaveChangesAsync();

        // Act
        var products = await _repository.GetAllAsync();

        // Assert
        products.Should().ContainSingle(p => p.Name == "Active");
        products.Should().NotContain(p => p.Name == "Deleted");
    }

    // ── SearchAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_WithTerm_ReturnsMatchingProducts()
    {
        // Arrange
        var laptop = new Product { Name = "Laptop Pro", Description = "Developer laptop", Price = 1299m, SKU = "LAP-001", Stock = 5 };
        var mouse  = new Product { Name = "Wireless Mouse", Description = "Ergonomic mouse", Price = 49m, SKU = "MOU-001", Stock = 20 };
        await _repository.AddAsync(laptop);
        await _repository.AddAsync(mouse);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("laptop", 1, 10);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle(p => p.Name == "Laptop Pro");
    }

    [Fact]
    public async Task SearchAsync_NullTerm_ReturnsAllProducts()
    {
        // Arrange
        await _repository.AddAsync(new Product { Name = "P1", Price = 1m, SKU = "P1", Stock = 1 });
        await _repository.AddAsync(new Product { Name = "P2", Price = 2m, SKU = "P2", Stock = 2 });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(null, 1, 10);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task SearchAsync_Pagination_ReturnsCorrectPage()
    {
        // Arrange — add 5 products
        for (int i = 1; i <= 5; i++)
            await _repository.AddAsync(new Product { Name = $"Product {i:D2}", Price = i, SKU = $"P{i:D2}", Stock = i });
        await _context.SaveChangesAsync();

        // Act — page 2, size 2
        var result = await _repository.SearchAsync(null, 2, 2);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.Page.Should().Be(2);
        result.TotalPages.Should().Be(3);
    }

    // ── Delete (soft) ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_SetsIsDeletedTrue_AndFiltersFromQueries()
    {
        // Arrange
        var product = new Product { Name = "To Delete", Price = 1m, SKU = "DEL-001", Stock = 0 };
        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        _repository.Delete(product);
        await _context.SaveChangesAsync();

        // Assert — global filter hides it
        var fetched = await _repository.GetByIdAsync(product.Id);
        fetched.Should().BeNull();

        // But it's still physically in DB
        var raw = await _context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == product.Id);
        raw.Should().NotBeNull();
        raw!.IsDeleted.Should().BeTrue();
    }
}

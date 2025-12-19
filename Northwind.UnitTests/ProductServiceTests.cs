using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Northwind.DataContext;
using Northwind.EntityModels;
using Northwind.Mvc.Services;

namespace Northwind.UnitTests;

public class ProductServiceTests
{
    private NorthwindContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NorthwindContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        return new NorthwindContext(options);
    }

    private IMemoryCache CreateCache() => new MemoryCache(new MemoryCacheOptions());

    [Fact]
    public async Task GetAllAsync_ExcludesDiscontinuedByDefault()
    {
        // Arrange
        using var context = CreateContext();
        context.Products.AddRange(
            new Product { ProductId = 1, ProductName = "Active", Discontinued = false },
            new Product { ProductId = 2, ProductName = "Discontinued", Discontinued = true }
        );
        await context.SaveChangesAsync();

        var service = new ProductService(context, CreateCache());

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().ProductName.Should().Be("Active");
    }

    [Fact]
    public async Task GetAllAsync_IncludesDiscontinuedWhenRequested()
    {
        // Arrange
        using var context = CreateContext();
        context.Products.AddRange(
            new Product { ProductId = 1, ProductName = "Active" },
            new Product { ProductId = 2, ProductName = "Discontinued", Discontinued = true }
        );
        await context.SaveChangesAsync();

        var service = new ProductService(context, CreateCache());

        // Act
        var result = await service.GetAllAsync(new ProductFilter { IncludeDiscontinued = true });

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByCategory()
    {
        // Arrange
        using var context = CreateContext();
        context.Products.AddRange(
            new Product { ProductId = 1, ProductName = "In Category", CategoryId = 1 },
            new Product { ProductId = 2, ProductName = "Other Category", CategoryId = 2 }
        );
        await context.SaveChangesAsync();

        var service = new ProductService(context, CreateCache());

        // Act
        var result = await service.GetAllAsync(new ProductFilter { CategoryId = 1 });

        // Assert
        result.Should().HaveCount(1);
        result.First().ProductName.Should().Be("In Category");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ReturnsProduct()
    {
        // Arrange
        using var context = CreateContext();
        context.Products.Add(new Product { ProductId = 1, ProductName = "Chai", UnitPrice = 18.00m });
        await context.SaveChangesAsync();

        var service = new ProductService(context, CreateCache());

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.ProductName.Should().Be("Chai");
        result.UnitPrice.Should().Be(18.00m);
    }

    [Fact]
    public async Task SearchAsync_FindsMatchingProducts()
    {
        // Arrange
        using var context = CreateContext();
        context.Products.AddRange(
            new Product { ProductId = 1, ProductName = "Green Tea" },
            new Product { ProductId = 2, ProductName = "Black Tea" },
            new Product { ProductId = 3, ProductName = "Coffee" }
        );
        await context.SaveChangesAsync();

        var service = new ProductService(context, CreateCache());

        // Act
        var result = await service.SearchAsync("Tea");

        // Assert
        result.Should().HaveCount(2);
        result.All(p => p.ProductName.Contains("Tea")).Should().BeTrue();
    }

    [Fact]
    public async Task GetNeedingReorderAsync_ReturnsLowStockProducts()
    {
        // Arrange
        using var context = CreateContext();
        context.Products.AddRange(
            new Product { ProductId = 1, ProductName = "Low Stock", UnitsInStock = 5, ReorderLevel = 10 },
            new Product { ProductId = 2, ProductName = "OK Stock", UnitsInStock = 50, ReorderLevel = 10 },
            new Product { ProductId = 3, ProductName = "Discontinued Low", UnitsInStock = 1, ReorderLevel = 10, Discontinued = true }
        );
        await context.SaveChangesAsync();

        var service = new ProductService(context, CreateCache());

        // Act
        var result = await service.GetNeedingReorderAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().ProductName.Should().Be("Low Stock");
    }
}

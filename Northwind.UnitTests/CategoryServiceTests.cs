using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Northwind.DataContext;
using Northwind.EntityModels;
using Northwind.Mvc.Services;

namespace Northwind.UnitTests;

public class CategoryServiceTests
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
    public async Task GetAllAsync_ReturnsAllCategories()
    {
        // Arrange
        using var context = CreateContext();
        context.Categories.AddRange(
            new Category { CategoryId = 1, CategoryName = "Beverages" },
            new Category { CategoryId = 2, CategoryName = "Condiments" }
        );
        await context.SaveChangesAsync();

        var service = new CategoryService(context, CreateCache());

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().CategoryName.Should().Be("Beverages");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCategory()
    {
        // Arrange
        using var context = CreateContext();
        context.Categories.Add(new Category { CategoryId = 1, CategoryName = "Beverages" });
        await context.SaveChangesAsync();

        var service = new CategoryService(context, CreateCache());

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.CategoryName.Should().Be("Beverages");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        var service = new CategoryService(context, CreateCache());

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWithProductCountsAsync_ReturnsCorrectCounts()
    {
        // Arrange
        using var context = CreateContext();
        var category = new Category { CategoryId = 1, CategoryName = "Beverages" };
        context.Categories.Add(category);
        context.Products.AddRange(
            new Product { ProductId = 1, ProductName = "Chai", CategoryId = 1 },
            new Product { ProductId = 2, ProductName = "Chang", CategoryId = 1 },
            new Product { ProductId = 3, ProductName = "Discontinued", CategoryId = 1, Discontinued = true }
        );
        await context.SaveChangesAsync();

        var service = new CategoryService(context, CreateCache());

        // Act
        var result = (await service.GetWithProductCountsAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].ProductCount.Should().Be(3);
        result[0].ActiveProductCount.Should().Be(2);
    }
}

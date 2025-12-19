using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Northwind.EntityModels;
using Northwind.Mvc.Controllers;
using Northwind.Mvc.Services;
using Northwind.Mvc.ViewModels;

namespace Northwind.UnitTests;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _productService;
    private readonly Mock<ICategoryService> _categoryService;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _productService = new Mock<IProductService>();
        _categoryService = new Mock<ICategoryService>();
        _controller = new ProductsController(_productService.Object, _categoryService.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewWithProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { ProductId = 1, ProductName = "Chai" },
            new() { ProductId = 2, ProductName = "Chang" }
        };
        _productService.Setup(s => s.GetAllAsync(It.IsAny<ProductFilter>())).ReturnsAsync(products);

        // Act
        var result = await _controller.Index(null);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ProductListViewModel>().Subject;
        model.Products.Should().HaveCount(2);
    }

    [Fact]
    public async Task Details_ValidId_ReturnsViewWithProduct()
    {
        // Arrange
        var product = new Product { ProductId = 1, ProductName = "Chai", CategoryId = 1 };
        _productService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);
        _productService.Setup(s => s.GetByCategoryAsync(1)).ReturnsAsync(new List<Product>());

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ProductDetailViewModel>().Subject;
        model.Product.ProductName.Should().Be("Chai");
    }

    [Fact]
    public async Task Details_NullId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Details(null);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task Details_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        _productService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.Details(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Search_EmptyQuery_ReturnsEmptyList()
    {
        // Act
        var result = await _controller.Search("");

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ProductListViewModel>().Subject;
        model.Products.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_ValidQuery_ReturnsMatchingProducts()
    {
        // Arrange
        var products = new List<Product> { new() { ProductId = 1, ProductName = "Chai" } };
        _productService.Setup(s => s.SearchAsync("Chai")).ReturnsAsync(products);

        // Act
        var result = await _controller.Search("Chai");

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ProductListViewModel>().Subject;
        model.Products.Should().HaveCount(1);
        model.SearchTerm.Should().Be("Chai");
    }

    [Fact]
    public async Task ByCategory_ValidId_ReturnsProductsInCategory()
    {
        // Arrange
        var category = new Category { CategoryId = 1, CategoryName = "Beverages" };
        var products = new List<Product> { new() { ProductId = 1, ProductName = "Chai" } };
        
        _categoryService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(category);
        _productService.Setup(s => s.GetByCategoryAsync(1)).ReturnsAsync(products);

        // Act
        var result = await _controller.ByCategory(1);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ProductListViewModel>().Subject;
        model.CurrentCategory!.CategoryName.Should().Be("Beverages");
        model.Products.Should().HaveCount(1);
    }

    [Fact]
    public async Task ByCategory_InvalidId_ReturnsNotFound()
    {
        // Arrange
        _categoryService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Category?)null);

        // Act
        var result = await _controller.ByCategory(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}

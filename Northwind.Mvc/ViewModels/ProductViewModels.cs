using Northwind.EntityModels;

namespace Northwind.Mvc.ViewModels;

public class ProductListViewModel
{
    public IEnumerable<Product> Products { get; set; } = [];
    public Category? CurrentCategory { get; set; }
    public string? SearchTerm { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;
}

public class ProductDetailViewModel
{
    public required Product Product { get; set; }
    public IEnumerable<Product> RelatedProducts { get; set; } = [];
}

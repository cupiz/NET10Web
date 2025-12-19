using Northwind.EntityModels;

namespace Northwind.Mvc.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync(ProductFilter? filter = null);
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> GetNeedingReorderAsync();
    Task<IEnumerable<Product>> SearchAsync(string term);
}

public class ProductFilter
{
    public int? CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public bool IncludeDiscontinued { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

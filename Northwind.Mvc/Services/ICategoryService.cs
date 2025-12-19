using Northwind.EntityModels;

namespace Northwind.Mvc.Services;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<IEnumerable<CategoryWithProductCount>> GetWithProductCountsAsync();
}

public record CategoryWithProductCount(
    int CategoryId,
    string CategoryName,
    string? Description,
    int ProductCount,
    int ActiveProductCount);

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Northwind.DataContext;
using Northwind.EntityModels;

namespace Northwind.Mvc.Services;

public class CategoryService(NorthwindContext db, IMemoryCache cache) : ICategoryService
{
    private const string CacheKey = "categories";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        if (cache.TryGetValue(CacheKey, out IEnumerable<Category>? cached) && cached != null)
            return cached;

        var categories = await db.Categories.AsNoTracking().OrderBy(c => c.CategoryName).ToListAsync();
        cache.Set(CacheKey, categories, CacheDuration);
        return categories;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        var key = $"category_{id}";
        if (cache.TryGetValue(key, out Category? cached))
            return cached;

        var category = await db.Categories.AsNoTracking()
            .Include(c => c.Products.Where(p => !p.Discontinued))
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category != null) cache.Set(key, category, CacheDuration);
        return category;
    }

    public async Task<IEnumerable<CategoryWithProductCount>> GetWithProductCountsAsync()
    {
        var key = "categories_counts";
        if (cache.TryGetValue(key, out IEnumerable<CategoryWithProductCount>? cached) && cached != null)
            return cached;

        var result = await db.Categories.AsNoTracking()
            .Select(c => new CategoryWithProductCount(
                c.CategoryId,
                c.CategoryName,
                c.Description,
                c.Products.Count,
                c.Products.Count(p => !p.Discontinued)))
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        cache.Set(key, result, CacheDuration);
        return result;
    }
}

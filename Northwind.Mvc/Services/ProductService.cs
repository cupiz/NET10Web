using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Northwind.Common;
using Northwind.DataContext;
using Northwind.EntityModels;

namespace Northwind.Mvc.Services;

public class ProductService(NorthwindContext db, IMemoryCache cache) : IProductService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<IEnumerable<Product>> GetAllAsync(ProductFilter? filter = null)
    {
        filter ??= new ProductFilter();
        var query = db.Products.AsNoTracking().Include(p => p.Category).Include(p => p.Supplier).AsQueryable();

        if (!filter.IncludeDiscontinued) query = query.Where(p => !p.Discontinued);
        if (filter.CategoryId.HasValue) query = query.Where(p => p.CategoryId == filter.CategoryId);
        if (filter.SupplierId.HasValue) query = query.Where(p => p.SupplierId == filter.SupplierId);
        if (filter.MinPrice.HasValue) query = query.Where(p => p.UnitPrice >= filter.MinPrice);
        if (filter.MaxPrice.HasValue) query = query.Where(p => p.UnitPrice <= filter.MaxPrice);
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm)) query = query.Where(p => p.ProductName.Contains(filter.SearchTerm));

        var pageSize = Math.Min(filter.PageSize, NorthwindConstants.MaxPageSize);
        return await query.OrderBy(p => p.ProductName).Skip((filter.Page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var key = $"product_{id}";
        if (cache.TryGetValue(key, out Product? cached)) return cached;

        var product = await db.Products.AsNoTracking()
            .Include(p => p.Category).Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product != null) cache.Set(key, product, CacheDuration);
        return product;
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        var key = $"products_cat_{categoryId}";
        if (cache.TryGetValue(key, out IEnumerable<Product>? cached) && cached != null) return cached;

        var products = await db.Products.AsNoTracking()
            .Include(p => p.Supplier)
            .Where(p => p.CategoryId == categoryId && !p.Discontinued)
            .OrderBy(p => p.ProductName).ToListAsync();

        cache.Set(key, products, CacheDuration);
        return products;
    }

    public async Task<IEnumerable<Product>> GetNeedingReorderAsync()
    {
        return await db.Products.AsNoTracking()
            .Include(p => p.Category).Include(p => p.Supplier)
            .Where(p => !p.Discontinued && p.UnitsInStock <= p.ReorderLevel)
            .OrderBy(p => p.UnitsInStock).ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term)) return [];
        return await db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.ProductName.Contains(term) && !p.Discontinued)
            .OrderBy(p => p.ProductName).Take(20).ToListAsync();
    }
}

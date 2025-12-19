using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Northwind.Mvc.Services;
using Northwind.Mvc.ViewModels;

namespace Northwind.Mvc.Controllers;

public class ProductsController(IProductService products, ICategoryService categories) : Controller
{
    public async Task<IActionResult> Index([FromQuery] ProductFilter? filter)
    {
        filter ??= new ProductFilter();
        var items = await products.GetAllAsync(filter);
        var vm = new ProductListViewModel
        {
            Products = items,
            SearchTerm = filter.SearchTerm,
            CurrentPage = filter.Page
        };

        if (filter.CategoryId.HasValue)
            vm.CurrentCategory = await categories.GetByIdAsync(filter.CategoryId.Value);

        return View(vm);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return BadRequest();
        var product = await products.GetByIdAsync(id.Value);
        if (product == null) return NotFound();

        var related = product.CategoryId.HasValue
            ? (await products.GetByCategoryAsync(product.CategoryId.Value)).Where(p => p.ProductId != id).Take(4)
            : [];

        return View(new ProductDetailViewModel { Product = product, RelatedProducts = related });
    }

    public async Task<IActionResult> Search(string? q)
    {
        if (string.IsNullOrWhiteSpace(q)) return View("Index", new ProductListViewModel());
        var items = await products.SearchAsync(q);
        return View("Index", new ProductListViewModel { Products = items, SearchTerm = q });
    }

    public async Task<IActionResult> ByCategory(int id)
    {
        var category = await categories.GetByIdAsync(id);
        if (category == null) return NotFound();
        var items = await products.GetByCategoryAsync(id);
        return View("Index", new ProductListViewModel { Products = items, CurrentCategory = category });
    }

    [Authorize]
    public async Task<IActionResult> ReorderAlerts()
    {
        var items = await products.GetNeedingReorderAsync();
        ViewData["Title"] = "Reorder Alerts";
        ViewData["IsReorderView"] = true;
        return View("Index", new ProductListViewModel { Products = items });
    }
}

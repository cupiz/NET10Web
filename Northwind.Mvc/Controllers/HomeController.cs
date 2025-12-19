using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Northwind.DataContext;
using Northwind.Mvc.Models;
using Northwind.Mvc.Services;
using Northwind.Mvc.ViewModels;
using System.Diagnostics;

namespace Northwind.Mvc.Controllers;

public class HomeController(ICategoryService categories, IProductService products, NorthwindContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var categoriesTask = categories.GetWithProductCountsAsync();
        var reorderTask = products.GetNeedingReorderAsync();
        var inventoryTask = db.Products.Where(p => !p.Discontinued)
            .SumAsync(p => (p.UnitPrice ?? 0) * (p.UnitsInStock ?? 0));
        var suppliersTask = db.Suppliers.CountAsync();

        await Task.WhenAll(categoriesTask, reorderTask, inventoryTask, suppliersTask);

        var cats = await categoriesTask;
        var vm = new HomeViewModel
        {
            CategoryCount = cats.Count(),
            ProductCount = cats.Sum(c => c.ActiveProductCount),
            SupplierCount = await suppliersTask,
            ReorderAlertCount = (await reorderTask).Count(),
            TotalInventoryValue = await inventoryTask,
            FeaturedCategories = cats.Take(4)
                .Select(c => new CategorySummary(c.CategoryId, c.CategoryName, c.Description, c.ActiveProductCount))
        };

        return View(vm);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}

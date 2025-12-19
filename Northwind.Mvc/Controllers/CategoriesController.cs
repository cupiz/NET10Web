using Microsoft.AspNetCore.Mvc;
using Northwind.Mvc.Services;

namespace Northwind.Mvc.Controllers;

public class CategoriesController(ICategoryService categories) : Controller
{
    public async Task<IActionResult> Index()
    {
        var items = await categories.GetWithProductCountsAsync();
        return View(items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return BadRequest();
        var category = await categories.GetByIdAsync(id.Value);
        return category == null ? NotFound() : View(category);
    }
}

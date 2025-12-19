using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Northwind.DataContext;
using Northwind.EntityModels;

namespace Northwind.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController(NorthwindContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var items = await db.Categories.AsNoTracking()
            .Select(c => new CategoryDto(c.CategoryId, c.CategoryName, c.Description, c.Products.Count))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Category), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await db.Categories.AsNoTracking()
            .Include(c => c.Products.Where(p => !p.Discontinued))
            .FirstOrDefaultAsync(c => c.CategoryId == id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize] // Protected
    [ProducesResponseType(typeof(Category), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
    {
        var category = new Category { CategoryName = dto.Name, Description = dto.Description };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, category);
    }

    [HttpPut("{id}")]
    [Authorize] // Protected
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryCreateDto dto)
    {
        var item = await db.Categories.FindAsync(id);
        if (item == null) return NotFound();
        item.CategoryName = dto.Name;
        item.Description = dto.Description;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Admin only
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.Categories.FindAsync(id);
        if (item == null) return NotFound();
        db.Categories.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record CategoryDto(int Id, string Name, string? Description, int ProductCount);
public record CategoryCreateDto(string Name, string? Description);

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Northwind.DataContext;
using Northwind.EntityModels;

namespace Northwind.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController(NorthwindContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? categoryId = null,
        [FromQuery] bool includeDiscontinued = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = db.Products.AsNoTracking().Include(p => p.Category).AsQueryable();
        if (!includeDiscontinued) query = query.Where(p => !p.Discontinued);
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId);

        var items = await query
            .OrderBy(p => p.ProductName)
            .Skip((page - 1) * pageSize)
            .Take(Math.Min(pageSize, 50))
            .Select(p => new ProductDto(
                p.ProductId, p.ProductName, p.UnitPrice, p.UnitsInStock,
                p.Discontinued, p.Category != null ? p.Category.CategoryName : null))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Product), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.ProductId == id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), 200)]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<ProductDto>());
        var items = await db.Products.AsNoTracking()
            .Where(p => p.ProductName.Contains(q) && !p.Discontinued)
            .Take(20)
            .Select(p => new ProductDto(p.ProductId, p.ProductName, p.UnitPrice, p.UnitsInStock, p.Discontinued, null))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("low-stock")]
    [Authorize] // Protected - requires JWT
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetLowStock()
    {
        var items = await db.Products.AsNoTracking()
            .Where(p => !p.Discontinued && p.UnitsInStock <= p.ReorderLevel)
            .OrderBy(p => p.UnitsInStock)
            .Select(p => new ProductDto(p.ProductId, p.ProductName, p.UnitPrice, p.UnitsInStock, p.Discontinued, null))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    [Authorize] // Protected - requires JWT
    [ProducesResponseType(typeof(Product), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
    {
        var product = new Product
        {
            ProductName = dto.Name,
            CategoryId = dto.CategoryId,
            SupplierId = dto.SupplierId,
            UnitPrice = dto.UnitPrice,
            UnitsInStock = dto.UnitsInStock,
            ReorderLevel = dto.ReorderLevel
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, product);
    }

    [HttpPut("{id}")]
    [Authorize] // Protected - requires JWT
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto)
    {
        var item = await db.Products.FindAsync(id);
        if (item == null) return NotFound();
        item.ProductName = dto.Name;
        item.UnitPrice = dto.UnitPrice;
        item.UnitsInStock = dto.UnitsInStock;
        item.ReorderLevel = dto.ReorderLevel;
        item.Discontinued = dto.Discontinued;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Protected - requires Admin role
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.Products.FindAsync(id);
        if (item == null) return NotFound();
        db.Products.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record ProductDto(int Id, string Name, decimal? Price, short? Stock, bool Discontinued, string? Category);
public record ProductCreateDto(string Name, int? CategoryId, int? SupplierId, decimal? UnitPrice, short? UnitsInStock, short? ReorderLevel);
public record ProductUpdateDto(string Name, decimal? UnitPrice, short? UnitsInStock, short? ReorderLevel, bool Discontinued);

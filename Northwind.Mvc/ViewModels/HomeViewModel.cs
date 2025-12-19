namespace Northwind.Mvc.ViewModels;

public class HomeViewModel
{
    public int CategoryCount { get; set; }
    public int ProductCount { get; set; }
    public int SupplierCount { get; set; }
    public int ReorderAlertCount { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public IEnumerable<CategorySummary> FeaturedCategories { get; set; } = [];
}

public record CategorySummary(int CategoryId, string CategoryName, string? Description, int ProductCount);

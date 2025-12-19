namespace Northwind.DataContext;

public static class DbInitializer
{
    public static async Task InitializeAsync(NorthwindContext context, bool force = false)
    {
        if (force) await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        if (await context.Categories.AnyAsync()) return;

        await SeedCategoriesAsync(context);
        await SeedSuppliersAsync(context);
        await SeedProductsAsync(context);
    }

    private static async Task SeedCategoriesAsync(NorthwindContext context)
    {
        var categories = new List<Category>
        {
            new() { CategoryName = "Beverages", Description = "Soft drinks, coffees, teas, beers, and ales" },
            new() { CategoryName = "Condiments", Description = "Sweet and savory sauces, relishes, spreads, and seasonings" },
            new() { CategoryName = "Confections", Description = "Desserts, candies, and sweet breads" },
            new() { CategoryName = "Dairy Products", Description = "Cheeses and other dairy products" },
            new() { CategoryName = "Grains/Cereals", Description = "Breads, crackers, pasta, and cereal" },
            new() { CategoryName = "Meat/Poultry", Description = "Prepared meats and poultry products" },
            new() { CategoryName = "Produce", Description = "Dried fruit and bean curd" },
            new() { CategoryName = "Seafood", Description = "Seaweed and fish products" }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSuppliersAsync(NorthwindContext context)
    {
        var suppliers = new List<Supplier>
        {
            new() { CompanyName = "Exotic Liquids", ContactName = "Charlotte Cooper", City = "London", Country = "UK", Phone = "(171) 555-2222" },
            new() { CompanyName = "New Orleans Cajun Delights", ContactName = "Shelley Burke", City = "New Orleans", Region = "LA", Country = "USA" },
            new() { CompanyName = "Grandma Kelly's Homestead", ContactName = "Regina Murphy", City = "Ann Arbor", Region = "MI", Country = "USA" },
            new() { CompanyName = "Tokyo Traders", ContactName = "Yoshi Nagase", City = "Tokyo", Country = "Japan" },
            new() { CompanyName = "Cooperativa de Quesos 'Las Cabras'", ContactName = "Antonio del Valle Saavedra", City = "Oviedo", Country = "Spain" }
        };

        await context.Suppliers.AddRangeAsync(suppliers);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(NorthwindContext context)
    {
        var beverages = await context.Categories.FirstAsync(c => c.CategoryName == "Beverages");
        var condiments = await context.Categories.FirstAsync(c => c.CategoryName == "Condiments");
        var seafood = await context.Categories.FirstAsync(c => c.CategoryName == "Seafood");
        var dairy = await context.Categories.FirstAsync(c => c.CategoryName == "Dairy Products");

        var exotic = await context.Suppliers.FirstAsync(s => s.CompanyName == "Exotic Liquids");
        var newOrleans = await context.Suppliers.FirstAsync(s => s.CompanyName == "New Orleans Cajun Delights");
        var tokyo = await context.Suppliers.FirstAsync(s => s.CompanyName == "Tokyo Traders");

        var products = new List<Product>
        {
            new() { ProductName = "Chai", SupplierId = exotic.SupplierId, CategoryId = beverages.CategoryId, QuantityPerUnit = "10 boxes x 20 bags", UnitPrice = 18.00m, UnitsInStock = 39, ReorderLevel = 10 },
            new() { ProductName = "Chang", SupplierId = exotic.SupplierId, CategoryId = beverages.CategoryId, QuantityPerUnit = "24 - 12 oz bottles", UnitPrice = 19.00m, UnitsInStock = 17, UnitsOnOrder = 40, ReorderLevel = 25 },
            new() { ProductName = "Aniseed Syrup", SupplierId = exotic.SupplierId, CategoryId = condiments.CategoryId, QuantityPerUnit = "12 - 550 ml bottles", UnitPrice = 10.00m, UnitsInStock = 13, ReorderLevel = 25 },
            new() { ProductName = "Chef Anton's Cajun Seasoning", SupplierId = newOrleans.SupplierId, CategoryId = condiments.CategoryId, QuantityPerUnit = "48 - 6 oz jars", UnitPrice = 22.00m, UnitsInStock = 53 },
            new() { ProductName = "Chef Anton's Gumbo Mix", SupplierId = newOrleans.SupplierId, CategoryId = condiments.CategoryId, QuantityPerUnit = "36 boxes", UnitPrice = 21.35m, UnitsInStock = 0, Discontinued = true },
            new() { ProductName = "Ikura", SupplierId = tokyo.SupplierId, CategoryId = seafood.CategoryId, QuantityPerUnit = "12 - 200 ml jars", UnitPrice = 31.00m, UnitsInStock = 31 },
            new() { ProductName = "Queso Cabrales", SupplierId = 5, CategoryId = dairy.CategoryId, QuantityPerUnit = "1 kg pkg.", UnitPrice = 21.00m, UnitsInStock = 22, UnitsOnOrder = 30, ReorderLevel = 30 },
            new() { ProductName = "Queso Manchego La Pastora", SupplierId = 5, CategoryId = dairy.CategoryId, QuantityPerUnit = "10 - 500 g pkgs.", UnitPrice = 38.00m, UnitsInStock = 86 }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}

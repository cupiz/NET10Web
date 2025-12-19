namespace Northwind.DataContext;

public static class NorthwindContextExtensions
{
    public static IServiceCollection AddNorthwindContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<NorthwindContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null);
                sql.CommandTimeout(30);
            });
#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        return services;
    }

    public static IServiceCollection AddNorthwindContextInMemory(this IServiceCollection services, string? dbName = null)
    {
        services.AddDbContext<NorthwindContext>(options =>
        {
            options.UseInMemoryDatabase(dbName ?? $"Northwind-{Guid.NewGuid()}");
#if DEBUG
            options.EnableSensitiveDataLogging();
#endif
        });

        return services;
    }

    public static async Task InitializeNorthwindDatabaseAsync(this IServiceProvider services, bool force = false)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NorthwindContext>();
        await DbInitializer.InitializeAsync(context, force);
    }
}

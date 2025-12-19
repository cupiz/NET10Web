using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Northwind.DataContext;
using Northwind.Mvc.Data;
using Northwind.Mvc.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Database
var connString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(connString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Northwind - using InMemory for demo
builder.Services.AddNorthwindContextInMemory("NorthwindDemo");

// Identity
builder.Services.AddDefaultIdentity<IdentityUser>(o =>
{
    o.SignIn.RequireConfirmedAccount = false;
    o.Password.RequireDigit = false;
    o.Password.RequireLowercase = false;
    o.Password.RequireUppercase = false;
    o.Password.RequireNonAlphanumeric = false;
    o.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Caching
builder.Services.AddMemoryCache();

// Localization
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(o =>
{
    var cultures = new[] { new CultureInfo("en-US"), new CultureInfo("id-ID") };
    o.DefaultRequestCulture = new RequestCulture("en-US");
    o.SupportedCultures = cultures;
    o.SupportedUICultures = cultures;
});

// MVC
builder.Services.AddControllersWithViews().AddViewLocalization().AddDataAnnotationsLocalization();

// Services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// Seed data
await app.Services.InitializeNorthwindDatabaseAsync();

// Pipeline
if (app.Environment.IsDevelopment())
    app.UseMigrationsEndPoint();
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.Run();

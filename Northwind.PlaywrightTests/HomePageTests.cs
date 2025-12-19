using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Northwind.PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class HomePageTests : PageTest
{
    private const string BaseUrl = "http://localhost:5000";

    [Test]
    public async Task HomePage_ShouldDisplayDashboard()
    {
        await Page.GotoAsync(BaseUrl);
        await Expect(Page).ToHaveTitleAsync(new Regex("Northwind"));
        await Expect(Page.Locator("text=Northwind Traders")).ToBeVisibleAsync();
    }

    [Test]
    public async Task HomePage_ShouldShowCategoryCount()
    {
        await Page.GotoAsync(BaseUrl);
        await Expect(Page.Locator("text=Categories")).ToBeVisibleAsync();
    }

    [Test]
    public async Task HomePage_ShouldShowProductCount()
    {
        await Page.GotoAsync(BaseUrl);
        await Expect(Page.Locator("text=Active Products")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Navigation_ProductsLink_ShouldWork()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.ClickAsync("text=Products");
        await Expect(Page).ToHaveURLAsync(new Regex("/Products"));
    }

    [Test]
    public async Task Navigation_CategoriesLink_ShouldWork()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.ClickAsync("text=Categories");
        await Expect(Page).ToHaveURLAsync(new Regex("/Categories"));
    }
}

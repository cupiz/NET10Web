using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Northwind.PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProductsPageTests : PageTest
{
    private const string BaseUrl = "http://localhost:5000";

    [Test]
    public async Task ProductsPage_ShouldDisplayProducts()
    {
        await Page.GotoAsync($"{BaseUrl}/Products");
        var cards = Page.Locator(".card");
        await Expect(cards.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task ProductsPage_SearchShouldWork()
    {
        await Page.GotoAsync($"{BaseUrl}/Products");
        await Page.FillAsync("input[name='q']", "Chai");
        await Page.ClickAsync("button[type='submit']");
        await Expect(Page.Locator("text=Chai")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ProductDetails_ShouldShowProductInfo()
    {
        await Page.GotoAsync($"{BaseUrl}/Products");
        await Page.ClickAsync("text=View Details >> nth=0");
        await Expect(Page.Locator("h1")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=Units in Stock")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CategoryFilter_ShouldWork()
    {
        await Page.GotoAsync($"{BaseUrl}/Categories");
        await Page.ClickAsync("text=View Products >> nth=0");
        await Expect(Page).ToHaveURLAsync(new Regex("/Products/ByCategory"));
    }
}

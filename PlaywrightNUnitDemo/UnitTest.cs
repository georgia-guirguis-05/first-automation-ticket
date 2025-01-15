using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightNUnitDemo;
[Parallelizable(ParallelScope.Self)] 
public class UnitTest1 : PageTest
{
    [SetUp]
    public async Task Setup()
    {
        await Page.GotoAsync("http://eaapp.somee.com/");
    }

    [Test]
    public async Task MyTest()
    {
 
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Page.GetByLabel("UserName").ClickAsync();
        await Page.GetByLabel("UserName").FillAsync("admin");
        await Page.GetByLabel("Password").ClickAsync();
        await Page.GetByLabel("Password").FillAsync("password");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Employee List" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Create New" }).ClickAsync();
        Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Employee" }));
    }
}
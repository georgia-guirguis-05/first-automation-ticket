using System.Diagnostics;

using Microsoft.Playwright;
using NUnit.Framework.Interfaces;

namespace ST_5336;

public class Tests
//create model, data loader, simplify by moving paths and kill process, figure out how to load a json file.

{
    private IPage _driver = null!;
    private string AppName = string.Empty;
    [SetUp]
    public async Task Setup()
    {
        KillShiftProcesses();
        var userDataDir = GetUserDataPath();
        var playwright = await Playwright.CreateAsync();
        var context = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir,
            new BrowserTypeLaunchPersistentContextOptions
            {
                ExecutablePath = GetShiftBrowserPath(),
                Headless = false,
                Args = ["--start-maximized"],
                ViewportSize = ViewportSize.NoViewport
            });

        _driver = await context.NewPageAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        var status = TestContext.CurrentContext.Result.Outcome.Status;
        if (TestStatus.Passed != status)
        {
            await _driver.ScreenshotAsync(new PageScreenshotOptions { Path = $"{AppName}.jpg" }); //add time stamp to name
        }
    }
    [TestCase("Facebook", "https://www.facebook.com/" ,"//button[@name='login']" )]
    [TestCase("Dropbox","https://www.dropbox.com/", "text='Log in'" )]
    [TestCase("LinkedIn","https://ca.linkedin.com/", "text='Sign in'" )]
    [TestCase("X","https://x.com/", "//span[text()='Join today.']" )]
    [TestCase("Instagram","https://www.instagram.com/", "text='Log in'" )]
    [TestCase("Asana","https://asana.com/", "//span[text()='Log In']")]
    [TestCase("WhatsApp","https://www.whatsapp.com/", "text='Log in'" )]
    [TestCase("Zoom","https://www.zoom.com/", "//a[text()='Sign In']" )]
    [TestCase("Canva","https://www.canva.com/", "text='Log in'" )]
    [TestCase("Spotify","https://open.spotify.com/", "text='Log in'" )]
    [TestCase("BBC","https://www.bbc.com/news", "text='Sign In'" )]
    [TestCase("GitHub","https://github.com/", "//input[@name='user_email']" )]
    [TestCase("GoogleDrive","https://workspace.google.com/intl/en-US/products/drive/", "text='Sign in'" )]
    [TestCase("NYT","https://www.nytimes.com/", "//span[text()='Continue']" )]
    [TestCase("Gmail","https://workspace.google.com/gmail/", "//a[contains(@class, 'header__aside__button')]//span[text()='Sign in']" )] 
    [TestCase("CNNNews","https://www.cnn.com/", "//div[@class='header__left']//button[@class='header__menu-icon']" )]
    
    public async Task LogIn(string appName, string url, string locator)
    {
        AppName = appName;
        await _driver.GotoAsync(url);
        await _driver.WaitForSelectorAsync(locator, new PageWaitForSelectorOptions
        {
            Timeout = 7000,
            State = WaitForSelectorState.Visible
        });
        
        Assert.That(await _driver.IsVisibleAsync(locator), Is.True, $"Failed to load {appName}.");
        
    }
    
    private static string GetShiftBrowserPath()
    {
        var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA")!;
        return Path.Combine(localAppData, "Shift", "chromium", "shift.exe");
    }

    private static string GetUserDataPath()
    {
        var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA")!;
        return Path.Combine(localAppData, "Shift", "User Data");
    }

    private static void KillShiftProcesses()
    {
        foreach (var proc in Process.GetProcessesByName("Shift"))
        {
            proc.Kill();
        }
    }
    
}
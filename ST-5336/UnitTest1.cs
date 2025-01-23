using System.Diagnostics;

using Microsoft.Playwright;

namespace ST_5336;

public class Tests
//create model, data loader, simplify by moving paths and kill process, figure out how to load a json file.

{
    private IPage _driver = null!;
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
    //
    // [TestCase("Facebook", "https://www.facebook.com/" ,"//button[@name='login']" )]
    // [TestCase("Dropbox","https://www.dropbox.com/", "text='Log in'" )]
    // [TestCase("LinkedIn","https://ca.linkedin.com/", "text='Sign in'" )]
    // [TestCase("X","https://x.com/", "//span[text()='Join today.']" )]
    // [TestCase("Instagram","https://www.instagram.com/", "text='Log in'" )]
    [TestCase("Asana","https://asana.com/", "//span[text()='Log In']")]
    // [TestCase("WhatsApp","https://www.whatsapp.com/", "text='Log in'" )]
    // [TestCase("Zoom","https://www.zoom.com/", "text='Sign In'" )]
    // [TestCase("NYT","https://www.nytimes.com/", "text='LOG IN'" )]
    // [TestCase("Canva","https://www.canva.com/", "text='Log in'" )]
    // [TestCase("Spotify","https://open.spotify.com/", "text='Log in'" )]
    // [TestCase("CNNNews","https://www.cnn.com/", "text='SIgn In'" )]
    // [TestCase("BBC","https://www.bbc.com/news", "text='Sign In'" )]
    // [TestCase("GitHub","https://github.com/", "text='Sign in'" )]
    // [TestCase("Gmail","https://workspace.google.com/gmail/", "//span[contains(text(),'Create an account')]" )]
    // [TestCase("GoogleDrive","https://workspace.google.com/intl/en-US/products/drive/", "text='Sign in'" )]
    public async Task LogIn(string appName, string url, string locator)
    { 
        
        await _driver.GotoAsync(url);
        // await _driver.WaitForURLAsync(url, new PageWaitForURLOptions
        // {
        //     Timeout = 7000,
        //     // WaitUntil = WaitUntilState.Load
        // });
        //
        await _driver.WaitForSelectorAsync(locator, new PageWaitForSelectorOptions
        {
            Timeout = 7000,
            State = WaitForSelectorState.Visible
        });

        try
        {
            Assert.That(await _driver.IsVisibleAsync(locator), Is.True, $"Failed to load {appName}.");
        }
        catch (AssertionException ex)
        {
            await _driver.ScreenshotAsync(new PageScreenshotOptions { Path = $"{appName}.jpg" });
            throw; // Re-throw the exception to ensure the test fails.
        }

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
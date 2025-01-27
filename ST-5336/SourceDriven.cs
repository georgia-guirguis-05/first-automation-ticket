using System.Diagnostics;
using System.Text.Json;
using Microsoft.Playwright;
using NUnit.Framework.Interfaces;

namespace ST_5336;

public class SourceDataTests
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
                Args = new[] { "--start-maximized" },
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
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // Format: YYYYMMDD_HHMMSS
            var fileName = $"{AppName}_{timestamp}.jpg";
            await _driver.ScreenshotAsync(new PageScreenshotOptions { Path = $"C:\\Users\\Georgia Guirguis\\Repo\\first-automation-ticket\\ST-5336\\failures\\{fileName}" });
        }
    }

    [Test]
    public async Task LogIn()
    {
        var testDataList = LoadTestData("C:\\Users\\Georgia Guirguis\\Repo\\first-automation-ticket\\ST-5336\\LandingPages.json");

        foreach (var testData in testDataList)
        {
            AppName = testData.AppName;

            await _driver.GotoAsync(testData.URL);
            await _driver.WaitForSelectorAsync(testData.Locator, new PageWaitForSelectorOptions
            {
                Timeout = 7000,
                State = WaitForSelectorState.Visible
            });

            Assert.That(await _driver.IsVisibleAsync(testData.Locator), Is.True, $"Failed to load {testData.AppName}.");
        }
    }

    private static List<LandingPageTestDataModel> LoadTestData(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<LandingPageTestDataModel>>(json) ?? new List<LandingPageTestDataModel>();
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


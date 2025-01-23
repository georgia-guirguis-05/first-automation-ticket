using Microsoft.Playwright;
using NUnit.Framework.Interfaces;


namespace ST_5336;

    [Category("WebTests")]
    public class BaseTestWeb
    {
        protected IPlaywright Playwright { get; private set; } = null!;
        protected IPage Driver { get; private set; } = null!;
        public IBrowserContext? Context { get; private set; }
        public byte[]? screenshotBytes;
        private readonly string shiftPath = @"C:\Users\Georgia Guirguis\AppData\Local\Shift\chromium\shift.exe";
        private readonly string userDataDir = @"C:\Users\Georgia Guirguis\AppData\Local\Shift\User Data";
        protected virtual bool MixpanelFlag { get; set; } = false;

        [SetUp]
        public async Task SetUp()
        {
            
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            var args = new[] { "--start-maximized" };

            if (MixpanelFlag)
            {
                const string mixpanelArgument = "--analytics-service-origin=http://localhost:8080/";
                args = [.. args, mixpanelArgument];
            }

            Context = await Playwright.Chromium.LaunchPersistentContextAsync(userDataDir,
                new BrowserTypeLaunchPersistentContextOptions
                {
                    ExecutablePath = shiftPath,
                    Headless = false,
                    Args = args,
                    ViewportSize = ViewportSize.NoViewport
                });

            await CloseFirstEmptyTabOnStart();
            Driver = await Context.NewPageAsync();

            var fullClassName = TestContext.CurrentContext.Test.ClassName;
            if (fullClassName != null && fullClassName.Contains('.'))
            {
                var className = fullClassName[(fullClassName.LastIndexOf('.') + 1)..];
                var testName = TestContext.CurrentContext.Test.MethodName;
            }
        }

        [TearDown]
        public async Task TearDown()
        {
            PrintResults();

            if (Driver != null)
            {
                await Driver.CloseAsync();
            }

            if (Context != null)
            {
                await Context.CloseAsync();
            }

            Playwright?.Dispose();
        }

        private static void PrintResults()
        {
            var testName = TestContext.CurrentContext.Test.Name;
            var testStatus = TestContext.CurrentContext.Result.Outcome.Status;

            var message = $"Test '{testName} finished with status: {testStatus}";
            TestContext.Progress.WriteLine(message);
        }

        private async Task CloseFirstEmptyTabOnStart()
        {
            if (Context != null)
            {
                var pages = Context.Pages;
                if (pages.Count > 0)
                {
                    await pages[0].CloseAsync();
                }
            }
        }
    }


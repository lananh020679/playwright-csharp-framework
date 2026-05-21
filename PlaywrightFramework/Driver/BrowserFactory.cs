using Microsoft.Playwright;
using PlaywrightFramework.Config;

namespace PlaywrightFramework.Driver
{
    public class BrowserFactory
    {
        private readonly BrowserSettings _settings;
        public BrowserFactory(BrowserSettings settings)
        {
            _settings=settings;
        }
        public async Task<IBrowser> CreateBrowserAsync(IPlaywright playwright)
        {
            var browserOption=new BrowserTypeLaunchOptions
            {
                Headless=_settings.Headless,
                SlowMo=_settings.SlowMo,
                Channel=_settings.DriverType switch
                {
                    DriverType.Chrome => "chrome",
                    DriverType.Edge => "msedge",
                    _=>null,
                },
            };

            var browserType=_settings.DriverType switch
            {
                DriverType.Chromium => playwright.Chromium,
                DriverType.Chrome => playwright.Chromium,
                DriverType.Edge => playwright.Chromium,
                DriverType.Firefox => playwright.Firefox,
                //DriverType.WebKit => playwright.WebKit,
                _ => playwright.Chromium,
            };

            return await browserType.LaunchAsync(browserOption);
        }
        public async Task<IBrowser[]> CreateMultipleBrowsersAsync(IPlaywright playwright, int count)
        {
            var tasks = new Task<IBrowser>[count];
            for (int i = 0; i < count; i++)
            {
                tasks[i] = CreateBrowserAsync(playwright);   // GEEN await — start alleen!
            }
            return await Task.WhenAll(tasks);   // wacht op alle parallel
        }
    }
}
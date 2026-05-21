using Microsoft.Playwright;
using PlaywrightFramework.Config;

namespace PlaywrightFramework.Driver
{
    public class BrowserContextFactory
    {
        private readonly ContextSettings  _contextSettings;
        public BrowserContextFactory(ContextSettings contextSettings)
        {
            _contextSettings=contextSettings;
        }
        public async Task<IBrowserContext> CreateBrowserContextAsync(IBrowser browser)
        {
            var browserNewContextOptions = new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize
                {
                    Width=_contextSettings.ViewportWidth,
                    Height=_contextSettings.ViewportHeight
                },
                Locale=_contextSettings.Locale,
                IgnoreHTTPSErrors=_contextSettings.IgnoreHttpsErrors,
                // Path policy lives in ContextSettings; the factory only translates. Null = no recording.
                RecordVideoDir = _contextSettings.RecordVideoDir,
                StorageStatePath = string.IsNullOrWhiteSpace(_contextSettings.AuthStatePath)
                    ? null
                    : _contextSettings.AuthStatePath
            };
            return await browser.NewContextAsync(browserNewContextOptions);
        }
    }
}

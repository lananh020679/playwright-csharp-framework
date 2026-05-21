using Microsoft.Playwright;

namespace PlaywrightFramework.Driver
{
    /// <summary>
    /// Orchestreert de initialisatie van een PlaywrightDriver.
    /// Roept de drie factories aan in de juiste volgorde.
    /// </summary>
    public class PlaywrightDriverInitializer
    {
        private readonly BrowserFactory _browserFactory;
        private readonly BrowserContextFactory _contextFactory;
        private readonly PageFactory _pageFactory;

        public PlaywrightDriverInitializer(
            BrowserFactory browserFactory,
            BrowserContextFactory contextFactory,
            PageFactory pageFactory)
        {
            _browserFactory = browserFactory;
            _contextFactory = contextFactory;
            _pageFactory = pageFactory;
        }

        public async Task<PlaywrightDriver> InitializeAsync()
        {
            var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            var browser    = await _browserFactory.CreateBrowserAsync(playwright);
            var context    = await _contextFactory.CreateBrowserContextAsync(browser);
            var page       = await _pageFactory.CreatePageAsync(context);

            return new PlaywrightDriver(playwright, browser, context, page);
        }
    }
}
using Microsoft.Playwright;

namespace PlaywrightFramework.Driver
{
    public class EagerDriverInitializer
    {
        private readonly Task<PlaywrightDriver> _driverTask;
        public EagerDriverInitializer(
            BrowserFactory browserFactory,
            BrowserContextFactory contextFactory,
            PageFactory pageFactory)
            {
                // EAGER: start nu, niet later. Geen await -> wordt fire-and-forget op thread pool.
                _driverTask = Task.Run(async () =>
                {
                    var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
                    var browser    = await browserFactory.CreateBrowserAsync(playwright);
                    var context    = await contextFactory.CreateBrowserContextAsync(browser);
                    var page       = await pageFactory.CreatePageAsync(context);
                    return new PlaywrightDriver(playwright, browser, context, page);
                });
            }
 
    // Consumers awaiten dit; als hij al klaar is komt het meteen terug.
    public Task<PlaywrightDriver> GetDriverAsync() => _driverTask;

    }
}
using Microsoft.Playwright;

namespace PlaywrightFramework.Driver
{
    public class PageFactory
    {
        private readonly int _defaultTimeoutMs;
        public PageFactory(int defaultTimeoutMs=30000)
        {
            _defaultTimeoutMs=defaultTimeoutMs;
        }
        public async Task<IPage> CreatePageAsync(IBrowserContext browserContext)
        {
            var page= await browserContext.NewPageAsync();
            page.SetDefaultTimeout(_defaultTimeoutMs);
            return page;
        }

    }
}
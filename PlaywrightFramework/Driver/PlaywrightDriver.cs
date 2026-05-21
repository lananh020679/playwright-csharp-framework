using Microsoft.Playwright;

namespace PlaywrightFramework.Driver
{
    ///<summary>
    ///    container Playwright objects.
    ///    No logical — only state. will be created door PlaywrightDriverInitializer.
    ///    1. IPlaywright Playwright spawns a Node.js (IPlaywright starts is the Node.js child process) IPlaywright = root driver process
    ///    2. IBrowser = real browser process
    ///    3. IBrowserContext = isolated profile/session
    ///    4. IPage = tab
    ///    => Step 1, 2 are mostly heavy they should be start one time in your process 
    ///</summary>
    public class PlaywrightDriver: IAsyncDisposable
    {
        public IPlaywright Playwright { get; }
        public IBrowser Browser { get; }
        public IBrowserContext BrowserContext { get; }
        public IPage Page { get; }

        private bool _disposed;
        public PlaywrightDriver(
            IPlaywright playwright,
            IBrowser browser,
            IBrowserContext context,
            IPage page)
        {
            Playwright = playwright;
            Browser = browser;
            BrowserContext = context;
            Page = page;
        }

        public async ValueTask  DisposeAsync()
        {
            Console.WriteLine($"DisposeAsync called. _disposed={_disposed}");
            if(_disposed) return;
            _disposed =true;

            // LIFO: Page -> Context -> Browser -> Playwright
            try
            {
                await Page.CloseAsync();
            }
            catch
            {
                /*swallow */
            } 
            try
            {
                await BrowserContext.CloseAsync();
            } 
            catch
            {
                /*swallow */
            }
            try
            {
                await Browser.CloseAsync();
            }
            catch
            {
                /*swallow */
            }
            try
            {
                Playwright.Dispose();
            }
            catch
            {
                /*swallow */
            }
            
            GC.SuppressFinalize(this);
            
        }
    }
    
}


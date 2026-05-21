
using Microsoft.Playwright;

namespace PlaywrightFramework.Driver
{
    /// <summary>
    /// Playwright-implementatie van IBrowserDriver.
    /// Alle pagina-objecten praten via deze interface; ze weten niet dat Playwright eronder zit.
    /// </summary>
    public class PlaywrightBrowserDriver: IBrowserDriver
    {
        private readonly IPage _page;
        public PlaywrightBrowserDriver(IPage page)
        {
            _page=page;
        }
        
        // --- Navigatie ---
        public async Task NavigateAsync(string url)
        {
            await _page.GotoAsync(url);
        }

        public async Task<string> GetTitleAsync()
        {
            return await _page.TitleAsync();
        }

        public async Task<string> GetCurrentUrlAsync()
        {
            return await Task.FromResult(_page.Url);
        }

        // --- Acties ---
        public async Task SelectByLabelAsync(string selector, string label)
        {
            var locator = _page.Locator(selector);

            await locator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await _page.WaitForFunctionAsync(
                @"selector => {
                    const select = document.querySelector(selector);
                    return select && select.options && select.options.length > 1;
                }",
                selector
            );

            await locator.SelectOptionAsync(new SelectOptionValue
            {
                Label = label
            });
        }

        public async Task SelectByValueAsync(string selector, string value)
        {
            var locator=_page.Locator(selector);
            await locator.WaitForAsync(new LocatorWaitForOptions 
            {
                State =WaitForSelectorState.Visible
            });
            await _page.WaitForFunctionAsync(
                @"selector => {
                     const select = document.querySelector(selector);
                    return select && select.options && select.options.length > 1;
                    }",
                selector
            );
            await locator.SelectOptionAsync(new SelectOptionValue
            {
                Value = value
                });
            
        }

        public async Task<IReadOnlyList<string>> GetSelectOptionsAsync(string selector)
        {
            return await _page.Locator($"{selector} option")
                .EvaluateAllAsync<string[]>(
                    "options => options.map(o => `${o.value} | ${o.textContent.trim()}`)"
                );
        }
        public async Task ClickAsync(string selector)
        {
            await _page.ClickAsync(selector);
        }

        public async Task FillAsync(string selector, string value)
        {
            await _page.FillAsync(selector, value);
        }

        // --- Queries ---
        public async Task<bool> IsVisibleAsync(string selector)
        {
            return await _page.Locator(selector).IsVisibleAsync();
        }

        public async Task<bool> IsVisibleAsync(string selector, int timeoutMs)
        {
            try
            {
                await _page.Locator(selector).WaitForAsync(new ()
                {
                    State=WaitForSelectorState.Visible,
                    Timeout=timeoutMs
                });
                return true;
            }
            catch(PlaywrightException ex) when (ex.Message.StartsWith("Timeout"))
            {
                return false;
            }
        }

        public async Task<string> GetTextAsync(string selector)
        {
            return await _page.Locator(selector).InnerTextAsync() ?? string.Empty;
        }
        public async Task<int> GetCountAsync(string selector)
        {
            return await _page.Locator(selector).CountAsync();
        }

        // --- Wachten ---
        public async Task WaitForVisibleAsync(string selector, int? timeoutMs = null)
        {
            var opts = new LocatorWaitForOptions { State = WaitForSelectorState.Visible };
            if (timeoutMs.HasValue) opts.Timeout = timeoutMs.Value;
            await _page.Locator(selector).WaitForAsync(opts);
        }

        // --- Lifecycle ---
        public async Task DisposeAsync()
        {
            await _page.CloseAsync();
        }
    }
}
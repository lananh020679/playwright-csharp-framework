using System.Diagnostics;

namespace PlaywrightFramework.Driver
{
    // Uses a small Intercept helper to avoid repeating the same try/catch/timing code in every method.
    // Screenshots-on-failure should eventually live in a Playwright-aware diagnostics layer or DI service,
    // because this decorator only knows IBrowserDriver and should not depend directly on IPage.
    public class LoggingBrowserDriver : IBrowserDriver
    {
        private readonly IBrowserDriver  _browserDriver;
        public LoggingBrowserDriver(IBrowserDriver browserDriver)
        {
            _browserDriver=browserDriver;
        }

        public Task NavigateAsync(string url)
            => InterceptAsync("NavigateAsync", url, () => _browserDriver.NavigateAsync(url));

        public Task<string> GetTitleAsync()
            => InterceptAsync("GetTitleAsync", "-", () => _browserDriver.GetTitleAsync());

        public Task<string> GetCurrentUrlAsync()
            => InterceptAsync("GetCurrentUrlAsync", "-", () => _browserDriver.GetCurrentUrlAsync());

        public Task ClickAsync(string selector)
            => InterceptAsync("ClickAsync", selector, () => _browserDriver.ClickAsync(selector));

        public Task FillAsync(string selector, string value)
        {
            // Do not log the value: it may contain passwords, tokens, or other sensitive data.
            return InterceptAsync("FillAsync", selector, () => _browserDriver.FillAsync(selector, value));
        }

        public Task SelectByLabelAsync(string selector, string label)
            => InterceptAsync("SelectByLabelAsync", selector, () => _browserDriver.SelectByLabelAsync(selector, label));

        public Task SelectByValueAsync(string selector, string value)
            => InterceptAsync("SelectByValueAsync", selector, () => _browserDriver.SelectByValueAsync(selector, value));

        public Task<IReadOnlyList<string>> GetSelectOptionsAsync(string selector)
            => InterceptAsync("GetSelectOptionsAsync", selector, () => _browserDriver.GetSelectOptionsAsync(selector));

        public Task<bool> IsVisibleAsync(string selector)
            => InterceptAsync("IsVisibleAsync", selector, () => _browserDriver.IsVisibleAsync(selector));

        public Task<bool> IsVisibleAsync(string selector, int timeoutMs)
            => InterceptAsync("IsVisibleAsync", selector, () => _browserDriver.IsVisibleAsync(selector, timeoutMs));

        public Task<string> GetTextAsync(string selector)
            => InterceptAsync("GetTextAsync", selector, () => _browserDriver.GetTextAsync(selector));

        public Task<int> GetCountAsync(string selector)
            => InterceptAsync("GetCountAsync", selector, () => _browserDriver.GetCountAsync(selector));

        public Task WaitForVisibleAsync(string selector, int? timeoutMs = null)
            => InterceptAsync("WaitForVisibleAsync", selector, () => _browserDriver.WaitForVisibleAsync(selector, timeoutMs));

        public Task DisposeAsync()
            => InterceptAsync("DisposeAsync", "-", () => _browserDriver.DisposeAsync());

        private static async Task InterceptAsync(string operation, string selector, Func<Task> action)
        {
            var stopwatch=Stopwatch.StartNew();
            try
            {
                Console.WriteLine($"[Browser] START {operation} selector='{selector}'");
                await action();
                stopwatch.Stop();
                Console.WriteLine($"[Browser] PASS  {operation} selector='{selector}' duration={stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"[Browser] FAIL  {operation} selector='{selector}' duration={stopwatch.ElapsedMilliseconds}ms error='{ex.Message}'");
                throw;
            }
        }

        private static async Task<T> InterceptAsync<T>(string operation, string selector, Func<Task<T>> action)
        {
            var stopwatch=Stopwatch.StartNew();
            try
            {
                Console.WriteLine($"[Browser] START {operation} selector='{selector}'");
                var result = await action();
                stopwatch.Stop();
                Console.WriteLine($"[Browser] PASS  {operation} selector='{selector}' duration={stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"[Browser] FAIL  {operation} selector='{selector}' duration={stopwatch.ElapsedMilliseconds}ms error='{ex.Message}'");
                throw;
            }
        }

    }
}
using System.Diagnostics;
using Microsoft.Playwright;
using NUnit.Framework;
using PlaywrightFramework.Config;
using PlaywrightFramework.Driver;

namespace PlaywrightTests.Tests
{
    [TestFixture]
    [NonParallelizable] 
    public class AsyncBrowserTests
    {
        [Test]
        public async Task Parallel_Is_Faster_Than_Sequential()
        {
            const int count=3;
            var settings = new BrowserSettings
            {
                Headless = true,
                DriverType = DriverType.Chromium
            };
            var factory = new BrowserFactory(settings);
            var playwright = await Microsoft.Playwright.Playwright.CreateAsync();

            // ─── Sequentieel ───
            var swSeq = Stopwatch.StartNew();
            var seqBrowsers = new IBrowser[count];
            for (int i = 0; i < count; i++)
            {
                seqBrowsers[i] = await factory.CreateBrowserAsync(playwright);
            }
            swSeq.Stop();

            foreach (var b in seqBrowsers) await b.CloseAsync();

            // ─── Parallel ───
            var swPar = Stopwatch.StartNew();
            var parBrowsers = await factory.CreateMultipleBrowsersAsync(playwright, count);
            swPar.Stop();

            foreach (var b in parBrowsers) await b.CloseAsync();
            playwright.Dispose();

            // ─── Rapport ───
            TestContext.WriteLine($"Sequentieel: {swSeq.ElapsedMilliseconds} ms");
            TestContext.WriteLine($"Parallel:    {swPar.ElapsedMilliseconds} ms");
            TestContext.WriteLine($"Snelheidswinst: {(double)swSeq.ElapsedMilliseconds / swPar.ElapsedMilliseconds:F2}x");

            Assert.That(swPar.ElapsedMilliseconds, Is.LessThan(swSeq.ElapsedMilliseconds),
                "Parallel zou sneller moeten zijn dan sequentieel.");
        }
    }
}